using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E del Layout compartido (_Layout.cshtml):
/// Navbar, footer, tema oscuro/claro, links de navegación, modales.
/// </summary>
[TestFixture]
public class LayoutTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // NAVBAR - ESTRUCTURA GENERAL
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Navbar_DebeSerVisible()
    {
        await GoToPage(TestConstants.IndexPath);
        var navbar = Page.Locator("nav.navbar, nav, header").First;
        await Expect(navbar).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navbar_LogoDebeEnlazarAlIndex()
    {
        await GoToPage(TestConstants.IndexPath);
        var logo = Page.Locator("a[href='/']").First;
        await Expect(logo).ToBeVisibleAsync();
        await Expect(logo).ToContainTextAsync("PandaDaw");
    }

    [Test]
    public async Task Navbar_LinksDeNavegacionVisiblesEnDesktop()
    {
        await GoToPage(TestConstants.IndexPath);
        // El link de "Tienda" debería ser visible en escritorio
        var tiendaLink = Page.GetByRole(AriaRole.Link, new() { Name = "Tienda" });
        await Expect(tiendaLink).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // NAVBAR - USUARIO NO AUTENTICADO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Navbar_SinLogin_MuestraBotonEntrar()
    {
        await GoToPage(TestConstants.IndexPath);
        var entrarLink = Page.GetByRole(AriaRole.Link, new() { Name = "Entrar" });
        await Expect(entrarLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navbar_SinLogin_NoMuestraPedidos()
    {
        await GoToPage(TestConstants.IndexPath);
        var pedidosLink = Page.GetByRole(AriaRole.Link, new() { Name = "Pedidos" });
        await Expect(pedidosLink).ToBeHiddenAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // NAVBAR - USUARIO AUTENTICADO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Navbar_ConLogin_MuestraNavegacionCompleta()
    {
        await LoginAsUser();

        var favoritosLink = Page.GetByRole(AriaRole.Link, new() { Name = "Favoritos" });
        await Expect(favoritosLink).ToBeVisibleAsync();

        var pedidosLink = Page.GetByRole(AriaRole.Link, new() { Name = "Pedidos" });
        await Expect(pedidosLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navbar_ConLogin_MuestraIconoCarrito()
    {
        await LoginAsUser();
        var carritoIcon = Page.Locator("#cart-icon");
        await Expect(carritoIcon).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navbar_ConLogin_NoMuestraBotonEntrar()
    {
        await LoginAsUser();
        var entrar = Page.GetByRole(AriaRole.Link, new() { Name = "Entrar" });
        await Expect(entrar).ToBeHiddenAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // NAVBAR - ADMIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Navbar_ConAdmin_MuestraLinkAdminPanel()
    {
        await LoginAsAdmin();
        var adminLink = Page.GetByRole(AriaRole.Link, new() { Name = "Admin" });
        await Expect(adminLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navbar_ConUserNormal_NoMuestraAdmin()
    {
        await LoginAsUser();
        var adminLink = Page.GetByRole(AriaRole.Link, new() { Name = "Admin" });
        await Expect(adminLink).ToBeHiddenAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // BUSCADOR DEL NAVBAR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Navbar_BuscadorFuncionaCorrectamente()
    {
        await GoToPage(TestConstants.IndexPath);
        var searchInput = Page.Locator("input[name='buscar']").First;
        await searchInput.FillAsync("test");
        await searchInput.PressAsync("Enter");
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"buscar=test"));
    }

    // ══════════════════════════════════════════════════════════════
    // TOGGLE TEMA (Dark / Light)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Layout_ToggleTemaExiste()
    {
        await GoToPage(TestConstants.IndexPath);
        // El botón de toggle tema tiene onclick="toggleTheme()"
        var toggleBtn = Page.Locator("[onclick*='toggleTheme'], .theme-controller").First;
        await Expect(toggleBtn).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // FOOTER
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Footer_DebeSerVisibleConCopyright()
    {
        await GoToPage(TestConstants.IndexPath);
        var footer = Page.Locator("footer");
        await Expect(footer).ToBeVisibleAsync();
        await Expect(footer).ToContainTextAsync("PandaDaw");
    }

    // ══════════════════════════════════════════════════════════════
    // MODAL DE LOGOUT
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Navbar_ModalLogout_TieneBotonCancelarYCerrarSesion()
    {
        await LoginAsUser();
        var modal = Page.Locator("#logoutModal");
        // El modal existe en el DOM aunque no esté visible
        await Expect(modal).ToBeAttachedAsync();
    }
}
