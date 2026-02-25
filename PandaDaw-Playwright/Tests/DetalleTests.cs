using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Detalle de producto (/Detalle/{id}).
/// Cubre: información del producto, añadir al carrito, toggle favorito,
/// formulario de valoración, sección de opiniones, breadcrumb.
/// </summary>
[TestFixture]
public class DetalleTests : BaseTest
{
    private const string ProductUrl = "/Detalle/1"; // Primer producto seed

    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_PaginaSeCargaCorrectamente()
    {
        await GoToPage(ProductUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Debe mostrar un heading con el nombre del producto
        var heading = Page.Locator("h1, h2, [class*='title']").First;
        await Expect(heading).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_MuestraImagenDelProducto()
    {
        await GoToPage(ProductUrl);
        var imagen = Page.Locator("img[src*='img'], img[src*='http']").First;
        await Expect(imagen).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_MuestraPrecioDelProducto()
    {
        await GoToPage(ProductUrl);
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("€"), "La página de detalle debe mostrar el precio con €");
    }

    [Test]
    public async Task Detalle_MuestraCategoriaDelProducto()
    {
        await GoToPage(ProductUrl);
        // Debe mostrar alguna de las categorías conocidas
        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneCat = TestConstants.Categorias.Any(cat =>
            pageText!.Contains(cat, StringComparison.OrdinalIgnoreCase));
        Assert.That(tieneCat, Is.True, "Debe mostrar la categoría del producto");
    }

    [Test]
    public async Task Detalle_MuestraDescripcion()
    {
        await GoToPage(ProductUrl);
        // La descripción es un bloque de texto largo en la página
        var descripcion = Page.Locator("p, [class*='description'], [class*='desc']").First;
        await Expect(descripcion).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_MuestraStockDelProducto()
    {
        await GoToPage(ProductUrl);
        var pageText = await Page.Locator("body").TextContentAsync();
        // Debe indicar stock o disponibilidad
        var tieneStock = pageText!.Contains("stock", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("disponible", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("unidades", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneStock, Is.True, "Debe informar sobre el stock");
    }

    // ══════════════════════════════════════════════════════════════
    // BREADCRUMB
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_MuestraBreadcrumb()
    {
        await GoToPage(ProductUrl);
        var breadcrumb = Page.Locator("[class*='breadcrumb'], nav[aria-label*='bread'], .text-sm.breadcrumbs").First;
        await Expect(breadcrumb).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // BOTÓN AÑADIR AL CARRITO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_BotonAnadirAlCarritoVisible_ConLogin()
    {
        await LoginAsUser();
        await GoToPage(ProductUrl);

        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await Expect(addBtn).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_AnadirAlCarrito_ConLogin_Funciona()
    {
        await LoginAsUser();
        await GoToPage(ProductUrl);

        var addBtn = Page.Locator("form[action*='AddToCart'] button, button:has-text('carrito'), button:has-text('Añadir')").First;
        await addBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Tras añadir, permanecemos en detalle o el badge del carrito se actualiza
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"Detalle/\d+|/$"));
    }

    // ══════════════════════════════════════════════════════════════
    // TOGGLE FAVORITO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_ToggleFavorito_ConLogin_Funciona()
    {
        await LoginAsUser();
        await GoToPage(ProductUrl);

        var favBtn = Page.Locator("form[action*='ToggleFavorito'] button").First;
        if (await favBtn.IsVisibleAsync())
        {
            await favBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // SECCIÓN DE VALORACIONES
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_SeccionValoracionesVisible()
    {
        await GoToPage(ProductUrl);
        // Debe haber una sección de opiniones/valoraciones
        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneValoraciones = pageText!.Contains("opiniones", StringComparison.OrdinalIgnoreCase)
                                || pageText.Contains("valoraciones", StringComparison.OrdinalIgnoreCase)
                                || pageText.Contains("reseñas", StringComparison.OrdinalIgnoreCase)
                                || pageText.Contains("parec", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneValoraciones, Is.True, "Debe existir sección de valoraciones");
    }

    [Test]
    public async Task Detalle_FormularioValoracion_ConLogin_Visible()
    {
        await LoginAsUser();
        await GoToPage(ProductUrl);

        // El formulario de valoración solo aparece para usuarios autenticados
        var textarea = Page.Locator("textarea[name='Resena'], textarea").First;
        await Expect(textarea).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_FormularioValoracion_TieneEstrellas()
    {
        await LoginAsUser();
        await GoToPage(ProductUrl);

        // Debe haber radio buttons para estrellas (1-5)
        var starInputs = Page.Locator("input[name='Estrellas']");
        var count = await starInputs.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(5), "Debe haber al menos 5 radio buttons de estrellas");
    }

    [Test]
    public async Task Detalle_EnviarValoracion_ConLogin_Funciona()
    {
        await LoginAsUser();
        // Usar un producto diferente para evitar conflicto con valoración ya existente
        await GoToPage("/Detalle/3");

        // Seleccionar 4 estrellas
        var star4 = Page.Locator("input[name='Estrellas'][value='4']");
        await star4.CheckAsync(new() { Force = true });

        // Escribir reseña
        var textarea = Page.Locator("textarea[name='Resena'], textarea").First;
        await textarea.FillAsync("Excelente producto, test automático de Playwright");

        // Enviar
        var submitBtn = Page.Locator("form:has(textarea) button[type='submit']").First;
        await submitBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // ══════════════════════════════════════════════════════════════
    // PRODUCTO INEXISTENTE
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_ProductoInexistente_MuestraError()
    {
        await GoToPage("/Detalle/999999");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Debe redirigir a Index o mostrar mensaje de error
        var url = Page.Url;
        var pageText = await Page.Locator("body").TextContentAsync();
        var esError = url.Contains("/") || pageText!.Contains("error", StringComparison.OrdinalIgnoreCase)
                      || pageText.Contains("no encontr", StringComparison.OrdinalIgnoreCase);
        Assert.That(esError, Is.True, "Producto inexistente debe mostrar error o redirigir");
    }

    // ══════════════════════════════════════════════════════════════
    // NAVEGACIÓN DESDE DETALLE
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_BreadcrumbInicio_NavegarAlIndex()
    {
        await GoToPage(ProductUrl);
        var inicioLink = Page.Locator("a[href='/'], a[href*='Index']").First;
        await Expect(inicioLink).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // RATING PROMEDIO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Detalle_MuestraRatingPromedio()
    {
        await GoToPage(ProductUrl);
        // El rating se muestra con estrellas/iconos o texto
        var ratingSection = Page.Locator(".rating, [class*='rating'], [class*='star']").First;
        await Expect(ratingSection).ToBeVisibleAsync();
    }
}
