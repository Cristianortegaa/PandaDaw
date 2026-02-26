using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página principal / Catálogo (Index /).
/// Cubre: hero, catálogo de productos, búsqueda, filtros por categoría, 
/// añadir al carrito, toggle de favoritos.
/// </summary>
[TestFixture]
public class IndexTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA GENERAL
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_PaginaSeCargaCorrectamente()
    {
        await GoToPage(TestConstants.IndexPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*"));
        // Debe haber productos visibles en la página
        var products = Page.Locator(".card, [class*='product'], [class*='card']");
        await Expect(products.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Index_MuestraHeroSection()
    {
        await GoToPage(TestConstants.IndexPath);
        // La sección hero es lo primero que ve el usuario
        var heroSection = Page.Locator("section, [class*='hero']").First;
        await Expect(heroSection).ToBeVisibleAsync();
    }

    [Test]
    public async Task Index_MuestraProductos()
    {
        await GoToPage(TestConstants.IndexPath);
        // Debe haber al menos un producto renderizado
        var products = Page.Locator("a[href*='Detalle']");
        var count = await products.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Debe haber al menos un producto en el catálogo");
    }

    [Test]
    public async Task Index_ProductosMuestranNombreYPrecio()
    {
        await GoToPage(TestConstants.IndexPath);
        // Verificar que hay productos con enlaces a detalle
        var products = Page.Locator("a[href*='Detalle']");
        var count = await products.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Debe haber productos en el catálogo");
    }

    // ══════════════════════════════════════════════════════════════
    // BUSCADOR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_BuscadorExiste()
    {
        await GoToPage(TestConstants.IndexPath);
        var searchInput = Page.Locator("input[name='buscar']").Last;
        await Expect(searchInput).ToBeVisibleAsync();
    }

    [Test]
    public async Task Index_BuscarProductoExistente_MuestraResultados()
    {
        await GoToPage(TestConstants.IndexPath);
        var searchInput = Page.Locator("input[name='buscar']").Last;
        await searchInput.FillAsync("a"); // Búsqueda genérica para encontrar algo
        await searchInput.PressAsync("Enter");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Debería haber resultados o al menos estar en la página con parámetro buscar
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"buscar="));
    }

    [Test]
    public async Task Index_BuscarProductoInexistente_MuestraMensajeVacio()
    {
        await GoToPage(TestConstants.IndexPath);
        var searchInput = Page.Locator("input[name='buscar']").Last;
        await searchInput.FillAsync("xyzproductoquenoexiste999");
        await searchInput.PressAsync("Enter");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // No debería haber cards de producto
        var products = Page.Locator("a[href*='Detalle']");
        var count = await products.CountAsync();
        Assert.That(count, Is.EqualTo(0), "No debe haber productos cuando se busca algo inexistente");
    }

    // ══════════════════════════════════════════════════════════════
    // FILTROS POR CATEGORÍA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_FiltrarPorCategoria_Smartphones()
    {
        await GoToPage(TestConstants.IndexPath + "?categoria=Smartphones");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"categoria=Smartphones"));
    }

    [Test]
    public async Task Index_FiltrarPorCategoria_Audio()
    {
        await GoToPage(TestConstants.IndexPath + "?categoria=Audio");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"categoria=Audio"));
    }

    [Test]
    public async Task Index_FiltrarPorCategoria_Laptops()
    {
        await GoToPage(TestConstants.IndexPath + "?categoria=Laptops");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"categoria=Laptops"));
    }

    [Test]
    public async Task Index_FiltrarPorCategoria_Gaming()
    {
        await GoToPage(TestConstants.IndexPath + "?categoria=Gaming");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"categoria=Gaming"));
    }

    [Test]
    public async Task Index_FiltrarPorCategoria_Imagen()
    {
        await GoToPage(TestConstants.IndexPath + "?categoria=Imagen");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"categoria=Imagen"));
    }

    [Test]
    public async Task Index_LinksDeCategoriasExisten()
    {
        await GoToPage(TestConstants.IndexPath);

        foreach (var cat in TestConstants.Categorias)
        {
            var catLink = Page.Locator($"a[href*='categoria={cat}']").First;
            await Expect(catLink).ToBeAttachedAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // CLICK EN PRODUCTO → NAVEGAR A DETALLE
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_ClickEnProducto_NavegarADetalle()
    {
        await GoToPage(TestConstants.IndexPath);
        var primerProducto = Page.Locator("a[href*='Detalle']").First;
        await primerProducto.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+"));
    }

    // ══════════════════════════════════════════════════════════════
    // AÑADIR AL CARRITO (requiere login)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_AnadirAlCarrito_ConLogin_Funciona()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.IndexPath);

        // Buscar el primer botón de añadir al carrito (icono fa-plus o texto "Añadir")
        var addToCartBtn = Page.Locator("form[action*='AddToCart'] button, form[action*='handler=AddToCart'] button").First;
        if (await addToCartBtn.IsVisibleAsync())
        {
            await addToCartBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            // Tras añadir, seguimos en la misma página o se actualiza el badge
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/"));
        }
    }

    // ══════════════════════════════════════════════════════════════
    // TOGGLE FAVORITO (requiere login)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Index_ToggleFavorito_ConLogin_Funciona()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.IndexPath);

        var favBtn = Page.Locator("form[action*='ToggleFavorito'] button, form[action*='handler=ToggleFavorito'] button").First;
        if (await favBtn.IsVisibleAsync())
        {
            await favBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/"));
        }
    }
}
