using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Login (/Login).
/// Cubre: formulario, validaciones, credenciales correctas/incorrectas, navegación.
/// </summary>
[TestFixture]
public class LoginTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA DE LA PÁGINA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_PaginaSeCargaCorrectamente()
    {
        await GoToPage(TestConstants.LoginPath);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex(".*"));
        await Expect(Page.Locator("#loginEmail")).ToBeVisibleAsync();
        await Expect(Page.Locator("#loginPassword")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_MuestraCampoEmailYPassword()
    {
        await GoToPage(TestConstants.LoginPath);
        var email = Page.Locator("#loginEmail");
        var password = Page.Locator("#loginPassword");

        await Expect(email).ToBeVisibleAsync();
        await Expect(email).ToHaveAttributeAsync("type", "email");
        await Expect(password).ToBeVisibleAsync();
        await Expect(password).ToHaveAttributeAsync("type", "password");
    }

    [Test]
    public async Task Login_MuestraBotonIniciarSesion()
    {
        await GoToPage(TestConstants.LoginPath);
        var boton = Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" });
        await Expect(boton).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_MuestraEnlaceARegistro()
    {
        await GoToPage(TestConstants.LoginPath);
        var registerLink = Page.GetByRole(AriaRole.Link, new() { Name = "Crear cuenta" })
                           ?? Page.GetByRole(AriaRole.Link, new() { Name = "Regístrate" })
                           ?? Page.Locator("a[href*='Register']");
        await Expect(registerLink!).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // TOGGLE PASSWORD (ojo)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_TogglePasswordCambiaVisibilidad()
    {
        await GoToPage(TestConstants.LoginPath);
        var passwordField = Page.Locator("#loginPassword");
        await Expect(passwordField).ToHaveAttributeAsync("type", "password");

        // Click en el botón de toggle (icono ojo)
        var toggleBtn = Page.Locator("[onclick*='togglePassword'][onclick*='loginPassword']");
        await toggleBtn.ClickAsync();

        await Expect(passwordField).ToHaveAttributeAsync("type", "text");
    }

    // ══════════════════════════════════════════════════════════════
    // LOGIN EXITOSO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_CredencialesCorrectas_RedirigeAlIndex()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("#loginEmail").FillAsync(TestConstants.UserEmail);
        await Page.Locator("#loginPassword").FillAsync(TestConstants.UserPassword);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$"));
    }

    [Test]
    public async Task Login_AdminCredenciales_RedirigeAlIndex()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("#loginEmail").FillAsync(TestConstants.AdminEmail);
        await Page.Locator("#loginPassword").FillAsync(TestConstants.AdminPassword);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$"));
    }

    // ══════════════════════════════════════════════════════════════
    // LOGIN FALLIDO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_CredencialesIncorrectas_MuestraError()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("#loginEmail").FillAsync("noexiste@pandadaw.com");
        await Page.Locator("#loginPassword").FillAsync("WrongPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        // Debe mostrar un mensaje de error
        var errorAlert = Page.Locator(".alert-error, .alert.alert-error, .text-error, [class*='error']").First;
        await Expect(errorAlert).ToBeVisibleAsync();
    }

    [Test]
    public async Task Login_PasswordIncorrecta_MuestraError()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("#loginEmail").FillAsync(TestConstants.UserEmail);
        await Page.Locator("#loginPassword").FillAsync("ContraseñaMal123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        var errorAlert = Page.Locator(".alert-error, .alert.alert-error, .text-error, [class*='error']").First;
        await Expect(errorAlert).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // VALIDACIONES DE FORMULARIO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_CamposVacios_NoPermiteEnviar()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        // Los campos tienen 'required', así que el formulario no se envía
        // y seguimos en la misma página
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    [Test]
    public async Task Login_EmailInvalido_NoPermiteEnviar()
    {
        await GoToPage(TestConstants.LoginPath);
        await Page.Locator("#loginEmail").FillAsync("correo-invalido");
        await Page.Locator("#loginPassword").FillAsync("password123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar sesión" }).ClickAsync();

        // El navegador valida type="email" y no permite enviar
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Login"));
    }

    // ══════════════════════════════════════════════════════════════
    // NAVEGACIÓN DESDE LOGIN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Login_EnlaceRegistro_NavegarARegister()
    {
        await GoToPage(TestConstants.LoginPath);
        var registerLink = Page.Locator("a[href*='Register']").First;
        await registerLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Register"));
    }
}
