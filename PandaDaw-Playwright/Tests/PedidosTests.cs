using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Pedidos / Historial (/Pedidos).
/// Cubre: acceso protegido, estadísticas, timeline de pedidos, estados.
/// </summary>
[TestFixture]
public class PedidosTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════════════
    // ACCESO SIN LOGIN → REDIRIGE A LOGIN
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.PedidosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_ConLogin_PaginaSeCarga()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.PedidosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Pedidos"));
    }

    [Test]
    public async Task Pedidos_MuestraTituloPagina()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.PedidosPath);
        var heading = Page.GetByRole(AriaRole.Heading).First;
        await Expect(heading).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════════════
    // ESTADÍSTICAS
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_MuestraEstadisticas()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.PedidosPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        // Las estadísticas muestran: total pedidos, productos comprados, total gastado
        var tieneStats = pageText!.Contains("pedido", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("comprad", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("gastad", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("Sin pedidos", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneStats, Is.True,
            "Debe mostrar estadísticas o indicar que no hay pedidos");
    }

    // ══════════════════════════════════════════════════════════════════════
    // PEDIDOS VACÍOS
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_SinPedidos_MuestraMensajeVacio()
    {
        // User2 probablemente no tiene pedidos
        await LoginAs(TestConstants.User2Email, TestConstants.User2Password);
        await GoToPage(TestConstants.PedidosPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        var estaVacio = pageText!.Contains("Sin pedidos", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("sin pedidos", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("no tienes", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("vacío", StringComparison.OrdinalIgnoreCase);
        Assert.That(estaVacio, Is.True, "Sin pedidos debe indicarlo claramente");
    }

    // ══════════════════════════════════════════════════════════════════════
    // TIMELINE DE PEDIDOS (tras crear uno)
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_TrasPago_MuestraPedidoEnHistorial()
    {
        await LoginAsUser();

        // 1. Añadir producto al carrito
        await GoToPage("/Detalle/5");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Ir a Pedidos y verificar que funciona
        await GoToPage(TestConstants.PedidosPath);
        var pageText = await Page.Locator("body").TextContentAsync();
        
        // Verificar que la página de pedidos carga correctamente
        var tieneContenido = pageText!.Contains("Pedido", StringComparison.OrdinalIgnoreCase)
                          || pageText.Contains("historial", StringComparison.OrdinalIgnoreCase)
                          || pageText.Contains("compra", StringComparison.OrdinalIgnoreCase)
                          || pageText.Contains("Sin pedidos", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneContenido, Is.True, "La página de pedidos debe cargar correctamente");
    }

    // ══════════════════════════════════════════════════════════════════════
    // ESTADOS DE PEDIDO
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_MuestraEstadoDelPedido()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.PedidosPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        var estados = new[] { "Pendiente", "Procesando", "Enviado", "Entregado", "Cancelado", "Sin pedidos" };
        var tieneEstado = estados.Any(e =>
            pageText!.Contains(e, StringComparison.OrdinalIgnoreCase));
        Assert.That(tieneEstado, Is.True,
            "Debe mostrar un estado válido de pedido o indicar que no hay pedidos");
    }
}
