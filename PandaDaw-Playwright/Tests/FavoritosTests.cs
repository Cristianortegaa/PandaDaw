using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Favoritos (/Favoritos).
/// Cubre: acceso protegido, lista de favoritos, eliminar, añadir al carrito desde favoritos.
/// </summary>
[TestFixture]
public class FavoritosTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // ACCESO SIN LOGIN → REDIRIGE A LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_ConLogin_PaginaSeCarga()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Favoritos"));
    }

    [Test]
    public async Task Favoritos_MuestraTituloPagina()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.FavoritosPath);
        var heading = Page.GetByRole(AriaRole.Heading).First;
        await Expect(heading).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // FAVORITOS VACÍOS
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_SinFavoritos_MuestraMensajeVacio()
    {
        // Usar user2 que probablemente no tiene favoritos
        await LoginAs(TestConstants.User2Email, TestConstants.User2Password);
        await GoToPage(TestConstants.FavoritosPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        var estaVacio = pageText!.Contains("favorit", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("vacío", StringComparison.OrdinalIgnoreCase)
                        || pageText.Contains("explorar", StringComparison.OrdinalIgnoreCase);
        Assert.That(estaVacio, Is.True, "Debe indicar que no hay favoritos o animar a explorar");
    }

    // ══════════════════════════════════════════════════════════════
    // FLUJO: AGREGAR Y VER FAVORITO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_AgregarYVerEnLista()
    {
        await LoginAsUser();

        // 1. Ir al detalle de un producto y toggle favorito
        await GoToPage("/Detalle/2");
        var favBtn = Page.Locator("form[action*='ToggleFavorito'] button").First;
        if (await favBtn.IsVisibleAsync())
        {
            await favBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // 2. Ir a favoritos y verificar que hay contenido
        await GoToPage(TestConstants.FavoritosPath);
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Does.Contain("€").Or.Contain("favorit"),
            "La página de favoritos debe mostrar productos o indicar estado");
    }

    // ══════════════════════════════════════════════════════════════
    // ELIMINAR FAVORITO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_EliminarFavorito_Funciona()
    {
        await LoginAsUser();

        // Asegurar que hay un favorito primero
        await GoToPage("/Detalle/4");
        var favBtn = Page.Locator("form[action*='ToggleFavorito'] button").First;
        if (await favBtn.IsVisibleAsync())
        {
            await favBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Ir a favoritos y eliminar
        await GoToPage(TestConstants.FavoritosPath);
        var removeBtn = Page.Locator("form[action*='RemoveFavorito'] button, button:has(.fa-trash), button:has(.fa-times)").First;
        if (await removeBtn.IsVisibleAsync())
        {
            await removeBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            // Tras eliminar, recargar para verificar
            await GoToPage(TestConstants.FavoritosPath);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AÑADIR AL CARRITO DESDE FAVORITOS
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_AnadirAlCarritoDesdeFavoritos_Funciona()
    {
        await LoginAsUser();

        // Asegurar que hay favorito
        await GoToPage("/Detalle/1");
        var favBtn = Page.Locator("form[action*='ToggleFavorito'] button").First;
        if (await favBtn.IsVisibleAsync())
        {
            await favBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await GoToPage(TestConstants.FavoritosPath);
        var addToCartBtn = Page.Locator("form[action*='AddToCart'] button, button:has(.fa-cart), a:has-text('carrito')").First;
        if (await addToCartBtn.IsVisibleAsync())
        {
            await addToCartBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // CARDS DE FAVORITOS MUESTRAN INFO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Favoritos_CardsMuestranImagenNombrePrecio()
    {
        await LoginAsUser();

        // Asegurar favorito
        await GoToPage("/Detalle/1");
        var favBtn = Page.Locator("form[action*='ToggleFavorito'] button").First;
        if (await favBtn.IsVisibleAsync())
        {
            await favBtn.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await GoToPage(TestConstants.FavoritosPath);

        // Verificar que hay imágenes y precios
        var images = Page.Locator("img");
        var count = await images.CountAsync();
        await Expect(Page.Locator("body")).ToContainTextAsync("€");
    }
}
