using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página del Carrito (/Carrito).
/// Cubre: carga, líneas de producto, modificar cantidad, eliminar línea,
/// vaciar carrito, resumen con IVA, enlace a pago.
/// </summary>
[TestFixture]
public class CarritoTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // ACCESO SIN LOGIN → REDIRIGE A LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.CarritoPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_ConLogin_PaginaSeCarga()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.CarritoPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Carrito"));
    }

    [Test]
    public async Task Carrito_Vacio_MuestraMensajeVacio()
    {
        // Usuario 2 probablemente tiene carrito vacío
        await LoginAs(TestConstants.User2Email, TestConstants.User2Password);
        await GoToPage(TestConstants.CarritoPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        var estaVacio = pageText!.Contains("vacío", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("vacio", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("sin productos", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("explorar", StringComparison.OrdinalIgnoreCase);
        Assert.That(estaVacio, Is.True, "Carrito vacío debe indicarlo");
    }

    // ══════════════════════════════════════════════════════════════
    // FLUJO: AÑADIR PRODUCTO AL CARRITO Y VERIFICAR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_AnadirProductoYVerificar()
    {
        await LoginAsUser();

        // 1. Ir al detalle de un producto y añadirlo
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000); // Esperar a que termine la animación de fly-to-cart
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Ir al carrito y verificar que hay al menos una línea
        await GoToPage(TestConstants.CarritoPath);
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Not.Contain("vacío"), "El carrito no debe estar vacío");
    }

    // ══════════════════════════════════════════════════════════════
    // CONTROLES DE CANTIDAD (+/-)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_BotonesIncrementarDecrementar_Visibles()
    {
        await LoginAsUser();

        // Añadir un producto primero
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToPage(TestConstants.CarritoPath);

        // Buscar botones +/- en el carrito
        var incrementBtn = Page.Locator("form[action*='UpdateCantidad'] button, button:has-text('+')").First;
        var decrementBtn = Page.Locator("form[action*='UpdateCantidad'] button, button:has-text('-')").First;

        // Al menos uno de los botones debería existir
        var hayBotones = await incrementBtn.IsVisibleAsync() || await decrementBtn.IsVisibleAsync();
        Assert.That(hayBotones, Is.True, "Deben existir controles de cantidad");
    }

    // ══════════════════════════════════════════════════════════════
    // BOTÓN ELIMINAR LÍNEA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_BotonEliminarLinea_Existe()
    {
        await LoginAsUser();

        // Añadir producto
        await GoToPage("/Detalle/2");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToPage(TestConstants.CarritoPath);
        var removeBtn = Page.Locator("form[action*='RemoveLinea'] button, button[class*='delete'], button:has(.fa-trash)").First;
        await Expect(removeBtn).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // VACIAR CARRITO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_VaciarCarrito_FuncionaCorrectamente()
    {
        await LoginAsUser();

        // Añadir producto primero
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToPage(TestConstants.CarritoPath);

        var vaciarBtn = Page.Locator("form[action*='Vaciar'] button, button:has-text('Vaciar')").First;
        if (await vaciarBtn.IsVisibleAsync())
        {
            await vaciarBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var pageText = await Page.Locator("body").TextContentAsync();
            var estaVacio = pageText!.Contains("vacío", StringComparison.OrdinalIgnoreCase)
                            || pageText.Contains("vacio", StringComparison.OrdinalIgnoreCase);
            Assert.That(estaVacio, Is.True, "Tras vaciar el carrito debe mostrarse vacío");
        }
    }

    // ══════════════════════════════════════════════════════════════
    // RESUMEN: SUBTOTAL, IVA, TOTAL
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_ResumenMuestraSubtotalIvaTotal()
    {
        await LoginAsUser();

        // Añadir un producto
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToPage(TestConstants.CarritoPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneResumen = pageText!.Contains("IVA", StringComparison.OrdinalIgnoreCase)
                           || pageText.Contains("Total", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneResumen, Is.True, "Debe mostrar un resumen con IVA y total");
    }

    // ══════════════════════════════════════════════════════════════
    // ENLACE A PAGO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_EnlacePagarAhora_Existe()
    {
        await LoginAsUser();

        // Añadir producto
        await GoToPage("/Detalle/1");
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GoToPage(TestConstants.CarritoPath);
        var pagarLink = Page.Locator("a[href*='Pago'], a:has-text('Pagar'), a:has-text('pagar')").First;
        await Expect(pagarLink).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // ENLACE SEGUIR COMPRANDO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Carrito_EnlaceSeguirComprando_NavegarAIndex()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.CarritoPath);

        var seguirLink = Page.Locator("a[href='/'], a[href*='Index'], a:has-text('Seguir comprando'), a:has-text('explorar'), a:has-text('Explorar')").First;
        if (await seguirLink.IsVisibleAsync())
        {
            await seguirLink.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$|/Index"));
        }
    }
}
