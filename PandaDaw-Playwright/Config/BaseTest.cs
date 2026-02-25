using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;

namespace PandaDaw_Playwright;

/// <summary>
/// Clase base de configuración para TODOS los tests de Playwright.
/// Hereda de PageTest (NUnit) y configura:
///   - Navegador en modo headed/headless
///   - Viewport fijo 1920x1080
///   - Locale es-ES y timezone Europe/Madrid
///   - Grabación de vídeo en results/videos
///   - Trazas completas (screenshots, snapshots, sources)
///   - Screenshots automáticos en cada test
/// </summary>
[TestFixture]
public abstract class BaseTest : PageTest
{
    // ── 1. Configuración de la Sesión (Context) ──
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid",
            ColorScheme = ColorScheme.Light,
            RecordVideoDir = TestConstants.VideosDir,
            IgnoreHTTPSErrors = true,
            BaseURL = TestConstants.BaseUrl,
        };
    }

    // ── 2. Inicio de trazas antes de cada test ──
    [SetUp]
    public async Task SetupTracing()
    {
        // Crear directorios de resultados si no existen
        Directory.CreateDirectory(TestConstants.ScreenshotsDir);
        Directory.CreateDirectory(TestConstants.VideosDir);
        Directory.CreateDirectory(TestConstants.TracesDir);

        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

    // ── 3. Guardar evidencias al terminar cada test ──
    [TearDown]
    public async Task TeardownEvidences()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var safeName = string.Join("_", testName.Split(Path.GetInvalidFileNameChars()));
        var status = TestContext.CurrentContext.Result.Outcome.Status;

        // Screenshot final siempre
        try
        {
            await Page.ScreenshotAsync(new()
            {
                Path = Path.Combine(TestConstants.ScreenshotsDir, $"{safeName}.png"),
                FullPage = true
            });
        }
        catch
        {
            // El navegador puede estar cerrado si el test falló críticamente
        }

        // Traza: guardar siempre (vital para CI/CD)
        await Context.Tracing.StopAsync(new()
        {
            Path = Path.Combine(TestConstants.TracesDir, $"{safeName}.zip")
        });
    }

    // ── Helpers de navegación base ──
    protected async Task GoToPage(string path)
    {
        await Page.GotoAsync(TestConstants.BaseUrl + path, new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    /// <summary>
    /// Inicia sesión con un usuario dado.
    /// </summary>
    protected async Task LoginAs(string email, string password)
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("#loginEmail").FillAsync(email);
        await Page.Locator("#loginPassword").FillAsync(password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();
        // Esperar a que redirija al index tras login exitoso
        await Page.WaitForURLAsync($"**{TestConstants.IndexPath}");
    }

    /// <summary>
    /// Inicia sesión como usuario normal (usuario1).
    /// </summary>
    protected async Task LoginAsUser()
    {
        await LoginAs(TestConstants.UserEmail, TestConstants.UserPassword);
    }

    /// <summary>
    /// Inicia sesión como administrador.
    /// </summary>
    protected async Task LoginAsAdmin()
    {
        await LoginAs(TestConstants.AdminEmail, TestConstants.AdminPassword);
    }

    /// <summary>
    /// Técnica de resiliencia: bloquea recursos externos innecesarios.
    /// </summary>
    protected async Task BlockExternalResources()
    {
        await Page.RouteAsync("**/*google-analytics*", async route => await route.AbortAsync());
        await Page.RouteAsync("**/*google*ads*", async route => await route.AbortAsync());
        await Page.RouteAsync("**/*doubleclick*", async route => await route.AbortAsync());
        await Page.RouteAsync("**/*adservice*", async route => await route.AbortAsync());
    }

    /// <summary>
    /// Técnica de resiliencia: limpieza quirúrgica del DOM.
    /// Elimina banners, overlays y popups que interfieran con los tests.
    /// </summary>
    protected async Task CleanDom()
    {
        await Page.EvaluateAsync(@"() => {
            const selectors = ['.adsbygoogle', '#promo-banner', '.newsletter-popup', '.cookie-banner'];
            selectors.forEach(sel => {
                document.querySelectorAll(sel).forEach(el => el.remove());
            });
        }");
    }
}
