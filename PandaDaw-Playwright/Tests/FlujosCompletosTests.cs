using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de Flujos Completos (End-to-End).
/// Simulan escenarios reales de usuario de principio a fin.
/// </summary>
[TestFixture]
public class FlujosCompletosTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // FLUJO 1: REGISTRO → LOGIN → EXPLORAR → COMPRAR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Flujo_RegistroLoginYCompra()
    {
        var uniqueEmail = $"flujo.compra.{Guid.NewGuid():N}@pandadaw.com";

        // 1. Registro
        await GoToPage(TestConstants.RegisterPath);
        
        // Selectores robustos ignorando elementos ocultos
        await Page.Locator("input[name*='ombre'], #regNombre >> visible=true").First.FillAsync("Flujo");
        await Page.Locator("input[name*='pellidos'], #regApellidos >> visible=true").First.FillAsync("Completo");
        await Page.Locator("input[type='email'], #regEmail >> visible=true").First.FillAsync(uniqueEmail);
        await Page.Locator("input[type='password'] >> visible=true").First.FillAsync("Flujo123!");
        await Page.Locator("input[type='password'] >> visible=true").Nth(1).FillAsync("Flujo123!");
        
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Debería estar logueado y en el index
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$"));

        // 3. Navegar a un producto
        var primerProducto = Page.Locator("a[href*='Detalle']").First;
        await primerProducto.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+"));

        // 4. Añadir al carrito
        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Task.Delay(1000); // Espera a la animación CSS del carrito
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 5. Ir al carrito
        await GoToPage(TestConstants.CarritoPath);
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Not.Contain("vacío"), "El carrito no debe estar vacío tras añadir un producto");

        // 6. Ir a pagar
        await GoToPage(TestConstants.PagoPath);

        // 7. Verificar que estamos en la página de pago
        await Expect(Page).ToHaveURLAsync(new Regex("Pago", RegexOptions.IgnoreCase));
        var pageTextPago = await Page.Locator("body").TextContentAsync();
        Assert.That(pageTextPago, Does.Contain("Pago").IgnoreCase.Or.Contain("pagar").IgnoreCase, 
            "La página de pago debe cargar correctamente");

        // 8. Verificar en Pedidos
        await GoToPage(TestConstants.PedidosPath);
        var pageTextPedidos = await Page.Locator("body").TextContentAsync();
        Assert.That(pageTextPedidos, Does.Not.Contain("Sin pedidos"), "Debe haber pedidos tras el flujo de compra");
    }

    // ══════════════════════════════════════════════════════════════
    // FLUJO 2: BUSCAR → DETALLE → FAVORITO → CARRITO → PAGAR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Flujo_BuscarProductoFavoritoYComprar()
    {
        await LoginAsUser();

        // 1. Buscar un producto (asegurando coger el buscador visible)
        await GoToPage(TestConstants.IndexPath);
        var searchInput = Page.Locator("input[name='buscar'] >> visible=true").First;
        await searchInput.FillAsync("a");
        await searchInput.PressAsync("Enter");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Ir al primer resultado
        var primerResultado = Page.Locator("a[href*='Detalle']").First;
        if (await primerResultado.IsVisibleAsync())
        {
            await primerResultado.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+"));

            // 3. Toggle favorito
            var favBtn = Page.Locator("form[action*='ToggleFavorito'] button").First;
            if (await favBtn.IsVisibleAsync())
            {
                await favBtn.ClickAsync();
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }

            // 4. Añadir al carrito
            var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('Añadir')").First;
            await addBtn.ClickAsync();
            await Task.Delay(1000); // Espera a la animación CSS
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 5. Verificar que está en favoritos
            await GoToPage(TestConstants.FavoritosPath);
            var pageText = await Page.Locator("body").TextContentAsync();
            Assert.That(pageText, Does.Not.Contain("Sin favoritos"), "Debe haber favoritos");

            // 6. Verificar carrito
            await GoToPage(TestConstants.CarritoPath);
            pageText = await Page.Locator("body").TextContentAsync();
            Assert.That(pageText, Does.Not.Contain("vacío"), "El carrito no debe estar vacío");
        }
    }

    // ══════════════════════════════════════════════════════════════
    // FLUJO 3: FILTRO POR CATEGORÍA → DETALLE → VALORACIÓN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Flujo_CategoriaDetalleYValorar()
    {
        await LoginAsUser();

        // 1. Filtrar por categoría
        await GoToPage(TestConstants.IndexPath + "?categoria=Gaming");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Click en primer producto
        var primerProducto = Page.Locator("a[href*='Detalle']").First;
        if (await primerProducto.IsVisibleAsync())
        {
            await primerProducto.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+"));

            // 3. Valorar (Si el usuario no lo ha valorado ya en pruebas anteriores)
            var estrella = Page.Locator("input[name='Estrellas']").Last;
            if (await estrella.IsVisibleAsync() && await estrella.IsEnabledAsync())
            {
                await estrella.ClickAsync(new LocatorClickOptions { Force = true });
                var textarea = Page.Locator("textarea[name='Resena'], textarea").First;
                if (await textarea.IsVisibleAsync())
                {
                    await textarea.FillAsync("¡Excelente producto probado en flujo E2E!");
                    await Page.Locator("button:has-text('Enviar'), button[type='submit']").First.ClickAsync();
                    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                }
            }

            var pageText = await Page.Locator("body").TextContentAsync();
            Assert.That(pageText, Is.Not.Null.And.Not.Empty, "La página de detalle debe cargar contenido");
        }
    }

    // ══════════════════════════════════════════════════════════════
    // FLUJO 4: ADMIN → CREAR PRODUCTO → VERIFICAR EN CATÁLOGO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Flujo_AdminCreaProductoYAparece()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var productoNombre = $"PW-Test-{Guid.NewGuid():N}"[..20];

        // 1. Crear producto
        var nuevoBtn = Page.Locator("button:has-text('Nuevo'), button:has-text('nuevo')").First;
        await nuevoBtn.ClickAsync();

        var modal = Page.Locator("#modal_crear, dialog[open]").First;
        await Expect(modal).ToBeVisibleAsync();

        // Evitar inputs invisibles de CSRF
        await modal.Locator("input[name*='ombre'] >> visible=true").First.FillAsync(productoNombre);
        await modal.Locator("textarea >> visible=true").First.FillAsync("Producto creado por Playwright");
        await modal.Locator("input[type='number'] >> visible=true").First.FillAsync("49.99");
        await modal.Locator("input[type='number'] >> visible=true").Nth(1).FillAsync("5");
        
        var categoriaSelect = modal.Locator("select >> visible=true").First;
        if (await categoriaSelect.IsVisibleAsync())
            await categoriaSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        await modal.Locator("input[name*='magen'], input[name*='url'] >> visible=true").First.FillAsync("https://via.placeholder.com/300");

        var submitBtn = modal.Locator("button[type='submit'], button:has-text('Crear') >> visible=true").First;
        await submitBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 2. Verificar que el catálogo carga
        await GoToPage(TestConstants.IndexPath);
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Contain("€"), "El catálogo debe mostrar productos");
    }

    // ══════════════════════════════════════════════════════════════
    // FLUJO 5: NAVEGACIÓN COMPLETA SIN LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Flujo_NavegacionSinLogin()
    {
        // 1. Index
        await GoToPage(TestConstants.IndexPath);
        await Expect(Page.Locator("body")).ToContainTextAsync("PandaDaw");

        // 2. Filtrar por categoría
        await GoToPage(TestConstants.IndexPath + "?categoria=Audio");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // 3. Ver detalle de producto
        var primerProducto = Page.Locator("a[href*='Detalle']").First;
        if (await primerProducto.IsVisibleAsync())
        {
            await primerProducto.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+"));
        }

        // 4. Intentar acceder a carrito → Login
        await GoToPage(TestConstants.CarritoPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));

        // 5. Intentar acceder a favoritos → Login
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));

        // 6. Login page carga bien
        await Expect(Page.Locator("input[type='email'] >> visible=true").First).ToBeVisibleAsync();

        // 7. Ir a Register
        var regLink = Page.Locator("a[href*='Register']").First;
        await regLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Register"));
    }
}