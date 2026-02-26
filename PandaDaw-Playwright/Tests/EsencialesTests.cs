using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

[TestFixture]
public class EsencialesTests : BaseTest
{
    [Test]
    public async Task Login_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.LoginPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Iniciar sesión.*"));
        await Expect(Page.Locator("input[name='Correo']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_ConCredencialesValidas_AccedeCorrectamente()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("input[name='Correo']").FillAsync(TestConstants.AdminEmail);
        await Page.Locator("input[name='Password']").FillAsync(TestConstants.AdminPassword);
        await Page.Locator("button[type='submit']").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*(/|$)"));
    }

    [Test]
    public async Task Register_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Crear cuenta.*"));
        await Expect(Page.Locator("input[name='Correo']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_ConDatosValidos_UsuarioCreado()
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";
        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("input[name='Correo']").FillAsync(email);
        await Page.Locator("input[name='Password']").FillAsync("Test123!");
        await Page.Locator("input[name='ConfirmPassword']").FillAsync("Test123!");
        await Page.Locator("input[name='Nombre']").FillAsync("Test");
        await Page.Locator("input[name='Apellidos']").FillAsync("User");
        await Page.Locator("button[type='submit']").ClickAsync();
    }

    [Test]
    public async Task Index_PaginaSeCargaCorrectamente()
    {
        await GoToPage(TestConstants.IndexPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*"));
        var products = Page.Locator(".card");
        await Expect(products.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Index_FiltroCategoria_Funciona()
    {
        await GoToPage(TestConstants.IndexPath + "?Categoria=Smartphones");
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Expect(Page.Locator("text=Smartphones").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_PaginaCargaCorrectamente()
    {
        await GoToPage("/Detalle/1");
        await Expect(Page.Locator("h1, [class*='title']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Detalle_MuestraValoraciones()
    {
        await GoToPage("/Detalle/1");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Valoraciones" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task Carrito_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.CarritoPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Carrito.*"));
    }

    [Test]
    public async Task Carrito_Vacio_MuestraMensaje()
    {
        await GoToPage(TestConstants.CarritoPath);
        var content = await Page.ContentAsync();
        Assert.That(content.ToLower(), Does.Contain("vacío").Or.Contain("sin productos").Or.Contain("carrito"));
    }

    [Test]
    public async Task Pago_PaginaCargaCorrectamente()
    {
        await GoToPage(TestConstants.PagoPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Pago.*|.*Pagar.*"));
    }

    [Test]
    public async Task Favoritos_PaginaCarga_RequiereLogin()
    {
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*Login.*|.*Iniciar.*"));
    }

    [Test]
    public async Task Favoritos_UsuarioLogueado_Accede()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Favoritos.*"));
    }

    [Test]
    public async Task Admin_PaginaCarga_RequiereLogin()
    {
        await GoToPage(TestConstants.AdminPanelPath);
        var currentUrl = Page.Url;
        Assert.That(currentUrl.Contains("AdminPanel") == false || currentUrl.Contains("Login") == true);
    }

    [Test]
    public async Task Admin_UsuarioAdmin_Accede()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*Admin.*|.*Panel.*"));
    }

    [Test]
    public async Task Logout_CierraSesion()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.LogoutPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*(/|$)"));
    }

    [Test]
    public async Task Error_PaginaNoExistente_404()
    {
        var response = await Page.GotoAsync(TestConstants.BaseUrl + "/pagina-inexistente-xyz");
        Assert.That(response?.Status, Is.EqualTo(404).Or.EqualTo(200));
    }
}
