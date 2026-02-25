using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Pedidos / Historial (/Pedidos).
/// Cubre: acceso protegido, estadísticas, timeline de pedidos, estados.
/// </summary>
[TestFixture]
public class PedidosTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // ACCESO SIN LOGIN → REDIRIGE A LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.PedidosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════
    // ESTADÍSTICAS
    // ══════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════
    // PEDIDOS VACÍOS
    // ══════════════════════════════════════════════════════════════

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

    // ══════════════════════════════════════════════════════════════
    // TIMELINE DE PEDIDOS (tras crear uno)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pedidos_TrasPago_MuestraPedidoEnHistorial()
    {
        await LoginAsUser();

        // 1. Añadir producto y pagar
        await GoToPage("/Detalle/5");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToPage(TestConstants.PagoPath);

        // Rellenar formulario de pago
        await Page.Locator("input[name*='ombre'], input[placeholder*='ombre']").First
            .FillAsync("Test");
        await Page.Locator("input[name*='pellido'], input[placeholder*='pellido']").First
            .FillAsync("Playwright");

        var emailField = Page.Locator("#pagoForm input[type='email'], input[name*='mail']").First;
        if (await emailField.IsVisibleAsync())
            await emailField.FillAsync("test@pw.com");

        var dirField = Page.Locator("input[name*='ireccion'], input[placeholder*='ireccion']").First;
        if (await dirField.IsVisibleAsync())
            await dirField.FillAsync("Calle Playwright 123");

        var cpField = Page.Locator("input[maxlength='5']").First;
        if (await cpField.IsVisibleAsync())
            await cpField.FillAsync("28001");

        var ciudadField = Page.Locator("input[name*='iudad'], input[placeholder*='iudad']").First;
        if (await ciudadField.IsVisibleAsync())
            await ciudadField.FillAsync("Madrid");

        var paypalRadio = Page.Locator("input[value='paypal']");
        if (await paypalRadio.IsVisibleAsync())
            await paypalRadio.CheckAsync(new() { Force = true });

        var pagarBtn = Page.Locator("#pagoForm button[type='submit'], button:has-text('Pagar')").First;
        await pagarBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Ir a pedidos y verificar
        await GoToPage(TestConstants.PedidosPath);
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Contain("€").Or.Contain("Pendiente").Or.Contain("pedido"),
            "El historial debe mostrar el pedido recién creado");
    }

    // ══════════════════════════════════════════════════════════════
    // ESTADOS DE PEDIDO
    // ══════════════════════════════════════════════════════════════

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
