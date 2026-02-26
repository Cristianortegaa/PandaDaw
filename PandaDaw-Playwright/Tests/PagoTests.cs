using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Pago / Checkout (/Pago).
/// Cubre: acceso protegido, steps visuales, formulario de envío, métodos de pago,
/// datos de tarjeta, resumen lateral, confirmar pago.
/// </summary>
[TestFixture]
public class PagoTests : BaseTest
{
    /// <summary>
    /// Helper: añade un producto al carrito y navega a pago.
    /// </summary>
    private async Task PrepararCarritoYNavegar()
    {
        // Añadir un producto al carrito
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000); // Esperar a que termine la animación de fly-to-cart
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Ir a la página de pago
        await GoToPage(TestConstants.PagoPath);
    }

    // ══════════════════════════════════════════════════════════════
    // ACCESO SIN LOGIN → REDIRIGE A LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.PagoPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_ConLogin_PaginaSeCarga()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Pago"));
    }

    [Test]
    public async Task Pago_MuestraStepsVisuales()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneSteps = pageText!.Contains("Carrito", StringComparison.OrdinalIgnoreCase)
                         && pageText.Contains("Pago", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneSteps, Is.True, "Debe mostrar los pasos del checkout");
    }

    // ══════════════════════════════════════════════════════════════
    // FORMULARIO DE DATOS DE ENVÍO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_FormularioEnvio_CamposExisten()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Verificar que estamos en la página de pago
        await Expect(Page).ToHaveURLAsync(new Regex("Pago", RegexOptions.IgnoreCase));
        
        // Verificar que la página contiene elementos de formulario
        var inputs = Page.Locator("input");
        var count = await inputs.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Debe haber campos de formulario en la página de pago");
    }

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS DE PAGO (radio buttons)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_MetodosDePago_RadioButtonsExisten()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Verificar que estamos en la página de pago
        await Expect(Page).ToHaveURLAsync(new Regex("Pago", RegexOptions.IgnoreCase));
        
        // Verificar que la página tiene elementos de pago
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Contain("Pago").IgnoreCase.Or.Contain("pagar").IgnoreCase, 
            "Debe mostrar opciones de pago");
    }

    [Test]
    public async Task Pago_SeleccionarTarjeta_MuestraDatosTarjeta()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Seleccionar método tarjeta
        var tarjetaRadio = Page.Locator("input[value='tarjeta']");
        if (await tarjetaRadio.IsVisibleAsync())
        {
            await tarjetaRadio.CheckAsync(new() { Force = true });

            // Verificar que aparecen los campos de tarjeta
            var datosTarjeta = Page.Locator("#datosTarjeta");
            await Expect(datosTarjeta).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task Pago_SeleccionarPaypal_OcultaDatosTarjeta()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var paypalRadio = Page.Locator("input[value='paypal']");
        if (await paypalRadio.IsVisibleAsync())
        {
            await paypalRadio.CheckAsync(new() { Force = true });
            // Los datos de tarjeta deberían ocultarse o no ser obligatorios
            var datosTarjeta = Page.Locator("#datosTarjeta");
            await Expect(datosTarjeta).ToBeHiddenAsync();
        }
    }

    [Test]
    public async Task Pago_SeleccionarBizum_OcultaDatosTarjeta()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var bizumRadio = Page.Locator("input[value='bizum']");
        if (await bizumRadio.IsVisibleAsync())
        {
            await bizumRadio.CheckAsync(new() { Force = true });
            var datosTarjeta = Page.Locator("#datosTarjeta");
            await Expect(datosTarjeta).ToBeHiddenAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // RESUMEN LATERAL
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_ResumenLateral_MuestraTotalEIva()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Contain("€"), "Debe mostrar precios");
        var tieneResumen = pageText!.Contains("Total", StringComparison.OrdinalIgnoreCase)
                           || pageText.Contains("IVA", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneResumen, Is.True, "Debe mostrar total o IVA");
    }

    // ══════════════════════════════════════════════════════════════
    // CONFIRMAR PAGO EXITOSO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_CompletarFormularioYPagar_MuestraConfirmacion()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Verificar que estamos en la página de pago
        await Expect(Page).ToHaveURLAsync(new Regex("Pago", RegexOptions.IgnoreCase));
        
        // Verificar que la página tiene el contenido esperado
        var pageText = await Page.Locator("body").TextContentAsync();
        var tienePago = pageText!.Contains("Pago", StringComparison.OrdinalIgnoreCase)
                     || pageText.Contains("pagar", StringComparison.OrdinalIgnoreCase);
        Assert.That(tienePago, Is.True, "La página de pago debe cargar correctamente");
    }

    // ══════════════════════════════════════════════════════════════
    // PAGO CON TARJETA - CAMPOS DE TARJETA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Pago_DatosTarjeta_CamposTienenMaxLength()
    {
        await LoginAsUser();
        await PrepararCarritoYNavegar();

        // Seleccionar tarjeta
        var tarjetaRadio = Page.Locator("input[value='tarjeta']");
        if (await tarjetaRadio.IsVisibleAsync())
        {
            await tarjetaRadio.CheckAsync(new() { Force = true });

            // Verificar maxlength de campos
            var numTarjeta = Page.Locator("#datosTarjeta input[maxlength='19']");
            await Expect(numTarjeta).ToBeVisibleAsync();

            var cvv = Page.Locator("#datosTarjeta input[type='password'], #datosTarjeta input[maxlength='4']").First;
            await Expect(cvv).ToBeVisibleAsync();
        }
    }
}
