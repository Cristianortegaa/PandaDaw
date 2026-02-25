using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de Resiliencia y Técnicas Avanzadas.
/// Cubre: bloqueo de publicidad, limpieza DOM, network idle,
/// force click, accesibilidad básica, responsive.
/// </summary>
[TestFixture]
public class ResilienciaTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // BLOQUEO DE RECURSOS EXTERNOS (Network Interception)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_BloqueoDeRecursosExternos_PaginaCargaCorrectamente()
    {
        // Activar bloqueo de publicidad y trackers
        await BlockExternalResources();

        await GoToPage(TestConstants.IndexPath);
        await Expect(Page.Locator("body")).ToContainTextAsync("PandaDaw");
    }

    // ══════════════════════════════════════════════════════════════
    // LIMPIEZA QUIRÚRGICA DEL DOM (DOM Scrubbing)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_LimpiezaDom_NoRompeLaPagina()
    {
        await GoToPage(TestConstants.IndexPath);

        // Ejecutar limpieza del DOM
        await CleanDom();

        // La página debe seguir funcionando tras la limpieza
        await Expect(Page.Locator("body")).ToContainTextAsync("PandaDaw");
        var products = Page.Locator("a[href*='Detalle']");
        var count = await products.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Los productos deben seguir visibles tras limpieza DOM");
    }

    // ══════════════════════════════════════════════════════════════
    // ESPERAS POR SILENCIO DE RED (Network Idle)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_NetworkIdle_CargaCompleta()
    {
        await Page.GotoAsync(TestConstants.BaseUrl + TestConstants.IndexPath, new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        // Si llegamos aquí, la red está en silencio → todo cargado
        await Expect(Page.Locator("body")).ToContainTextAsync("PandaDaw");
    }

    // ══════════════════════════════════════════════════════════════
    // FORCE CLICK (Bypass de Overlays)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_ForceClick_FuncionaEnBotones()
    {
        await GoToPage(TestConstants.IndexPath);

        // Intentar forzar un click en el primer enlace (simula overlay bloqueando)
        var primerLink = Page.Locator("a[href*='Detalle']").First;
        await primerLink.ClickAsync(new() { Force = true });
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+"));
    }

    // ══════════════════════════════════════════════════════════════
    // INYECCIÓN DE JAVASCRIPT PARA LIMPIAR ELEMENTOS
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_EvaluateJs_EliminaElementosIndeseados()
    {
        await GoToPage(TestConstants.IndexPath);

        // Inyectar JS para eliminar iframes si los hay
        await Page.EvaluateAsync(@"() => {
            const ads = document.querySelectorAll('iframe, .adsbygoogle');
            ads.forEach(ad => ad.remove());
        }");

        // La página debe seguir operativa
        var products = Page.Locator("a[href*='Detalle']");
        var count = await products.CountAsync();
        Assert.That(count, Is.GreaterThan(0));
    }

    // ══════════════════════════════════════════════════════════════
    // INTERCEPTAR RED PARA VERIFICAR PETICIONES
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_InterceptarRed_PeticionesCorrectas()
    {
        var peticiones = new List<string>();

        // Registrar todas las peticiones de la página
        Page.Request += (_, request) =>
        {
            peticiones.Add(request.Url);
        };

        await GoToPage(TestConstants.IndexPath);

        // Verificar que hubo peticiones al servidor local
        Assert.That(peticiones.Count, Is.GreaterThan(0), "Debe haber peticiones de red");
        Assert.That(peticiones.Any(p => p.Contains("localhost")), Is.True,
            "Las peticiones deben apuntar a localhost");
    }

    // ══════════════════════════════════════════════════════════════
    // RESPONSIVE: VIEWPORT MÓVIL
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_ViewportMovil_PaginaCargaCorrectamente()
    {
        await Page.SetViewportSizeAsync(375, 812); // iPhone X
        await GoToPage(TestConstants.IndexPath);
        await Expect(Page.Locator("body")).ToContainTextAsync("PandaDaw");
    }

    [Test]
    public async Task Resiliencia_ViewportTablet_PaginaCargaCorrectamente()
    {
        await Page.SetViewportSizeAsync(768, 1024); // iPad
        await GoToPage(TestConstants.IndexPath);
        await Expect(Page.Locator("body")).ToContainTextAsync("PandaDaw");
    }

    // ══════════════════════════════════════════════════════════════
    // ACCESIBILIDAD BÁSICA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_PaginaTieneTitulo()
    {
        await GoToPage(TestConstants.IndexPath);
        var title = await Page.TitleAsync();
        Assert.That(title, Is.Not.Empty, "Toda página debe tener un título");
    }

    [Test]
    public async Task Resiliencia_ImagenesTienenAlt()
    {
        await GoToPage(TestConstants.IndexPath);
        var images = Page.Locator("img");
        var count = await images.CountAsync();

        for (int i = 0; i < Math.Min(count, 5); i++) // Revisar las primeras 5 imágenes
        {
            var alt = await images.Nth(i).GetAttributeAsync("alt");
            // alt debería existir (puede estar vacío para imágenes decorativas)
            Assert.That(alt, Is.Not.Null, $"La imagen {i} debería tener atributo alt");
        }
    }

    [Test]
    public async Task Resiliencia_FormulariosUsanLabels()
    {
        await GoToPage(TestConstants.LoginPath);
        var labels = Page.Locator("label");
        var count = await labels.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Los formularios deben usar labels");
    }

    // ══════════════════════════════════════════════════════════════
    // RENDIMIENTO BÁSICO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Resiliencia_PaginaCargaEnTiempoRazonable()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await GoToPage(TestConstants.IndexPath);
        stopwatch.Stop();

        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(30000),
            "La página debe cargar en menos de 30 segundos");
    }
}
