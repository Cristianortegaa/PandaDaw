using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de Logout (/Logout).
/// Cubre: cerrar sesión, redirigir al index, limpiar sesión.
/// </summary>
[TestFixture]
public class LogoutTests : BaseTest
{
    [Test]
    public async Task Logout_CierraSesionYRedirigeAlIndex()
    {
        await LoginAsUser();

        // Navegar directamente a /Logout
        await GoToPage(TestConstants.LogoutPath);

        // Debe redirigir al index
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$|/Index"));
    }

    [Test]
    public async Task Logout_TrasCerrarSesion_NoPuedeAccederACarrito()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.LogoutPath);

        // Intentar acceder al carrito → debe redirigir a Login
        await GoToPage(TestConstants.CarritoPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    [Test]
    public async Task Logout_TrasCerrarSesion_NoPuedeAccederAFavoritos()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.LogoutPath);

        await GoToPage(TestConstants.FavoritosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    [Test]
    public async Task Logout_TrasCerrarSesion_NoPuedeAccederAPedidos()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.LogoutPath);

        await GoToPage(TestConstants.PedidosPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    [Test]
    public async Task Logout_TrasCerrarSesion_NavbarMuestraBotonEntrar()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.LogoutPath);

        // Ir al index y verificar que aparece "Entrar"
        await GoToPage(TestConstants.IndexPath);
        var entrarLink = Page.GetByRole(AriaRole.Link, new() { Name = "Entrar" });
        await Expect(entrarLink).ToBeVisibleAsync();
    }
}
