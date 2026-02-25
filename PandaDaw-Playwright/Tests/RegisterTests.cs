using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Registro (/Register).
/// Cubre: formulario, validaciones, registro exitoso, password toggle.
/// </summary>
[TestFixture]
public class RegisterTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // CARGA Y ESTRUCTURA DE LA PÁGINA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Register_PaginaSeCargaCorrectamente()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Expect(Page.Locator("#regNombre")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regApellidos")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regEmail")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regPassword")).ToBeVisibleAsync();
        await Expect(Page.Locator("#regConfirm")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_MuestraBotonCrearCuenta()
    {
        await GoToPage(TestConstants.RegisterPath);
        var boton = Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" });
        await Expect(boton).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_CamposTienenAtributosCorrectos()
    {
        await GoToPage(TestConstants.RegisterPath);

        await Expect(Page.Locator("#regNombre")).ToHaveAttributeAsync("type", "text");
        await Expect(Page.Locator("#regApellidos")).ToHaveAttributeAsync("type", "text");
        await Expect(Page.Locator("#regEmail")).ToHaveAttributeAsync("type", "email");
        await Expect(Page.Locator("#regPassword")).ToHaveAttributeAsync("type", "password");
        await Expect(Page.Locator("#regConfirm")).ToHaveAttributeAsync("type", "password");
    }

    // ══════════════════════════════════════════════════════════════
    // TOGGLE PASSWORD
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Register_TogglePasswordFunciona()
    {
        await GoToPage(TestConstants.RegisterPath);
        var passwordField = Page.Locator("#regPassword");
        await Expect(passwordField).ToHaveAttributeAsync("type", "password");

        var toggleBtn = Page.Locator("[onclick*='togglePassword'][onclick*='regPassword']");
        await toggleBtn.ClickAsync();
        await Expect(passwordField).ToHaveAttributeAsync("type", "text");
    }

    [Test]
    public async Task Register_ToggleConfirmPasswordFunciona()
    {
        await GoToPage(TestConstants.RegisterPath);
        var confirmField = Page.Locator("#regConfirm");
        await Expect(confirmField).ToHaveAttributeAsync("type", "password");

        var toggleBtn = Page.Locator("[onclick*='togglePassword'][onclick*='regConfirm']");
        await toggleBtn.ClickAsync();
        await Expect(confirmField).ToHaveAttributeAsync("type", "text");
    }

    // ══════════════════════════════════════════════════════════════
    // VALIDACIONES DE FORMULARIO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Register_CamposVacios_NoPermiteEnviar()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();

        // Los campos required impiden el envío → seguimos en Register
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Register"));
    }

    [Test]
    public async Task Register_EmailInvalido_NoPermiteEnviar()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("#regNombre").FillAsync("Test");
        await Page.Locator("#regApellidos").FillAsync("User");
        await Page.Locator("#regEmail").FillAsync("email-invalido");
        await Page.Locator("#regPassword").FillAsync("Test123!");
        await Page.Locator("#regConfirm").FillAsync("Test123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Register"));
    }

    [Test]
    public async Task Register_PasswordsNoCoinciden_MuestraError()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("#regNombre").FillAsync("Test");
        await Page.Locator("#regApellidos").FillAsync("User");
        await Page.Locator("#regEmail").FillAsync("test.mismatch@pandadaw.com");
        await Page.Locator("#regPassword").FillAsync("Test123!");
        await Page.Locator("#regConfirm").FillAsync("OtroPassword123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();

        // Debe mostrar error de passwords no coinciden o quedarse en Register
        var errorAlert = Page.Locator(".alert-error, .alert.alert-error, .text-error, span[class*='error'], [class*='validation']").First;
        await Expect(errorAlert).ToBeVisibleAsync();
    }

    [Test]
    public async Task Register_PasswordDebil_MuestraError()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("#regNombre").FillAsync("Test");
        await Page.Locator("#regApellidos").FillAsync("User");
        await Page.Locator("#regEmail").FillAsync("test.weak@pandadaw.com");
        await Page.Locator("#regPassword").FillAsync("123");
        await Page.Locator("#regConfirm").FillAsync("123");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();

        // Se queda en Register con error de validación
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("Register"));
    }

    [Test]
    public async Task Register_EmailYaExiste_MuestraError()
    {
        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("#regNombre").FillAsync("Duplicado");
        await Page.Locator("#regApellidos").FillAsync("User");
        await Page.Locator("#regEmail").FillAsync(TestConstants.UserEmail); // Ya existe
        await Page.Locator("#regPassword").FillAsync("Duplicado123!");
        await Page.Locator("#regConfirm").FillAsync("Duplicado123!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();

        // Debe mostrar error de email duplicado
        var errorAlert = Page.Locator(".alert-error, .alert.alert-error, .text-error, [class*='error']").First;
        await Expect(errorAlert).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // REGISTRO EXITOSO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Register_DatosValidos_RegistraYRedirigeAlIndex()
    {
        var uniqueEmail = $"test.pw.{Guid.NewGuid():N}@pandadaw.com";

        await GoToPage(TestConstants.RegisterPath);
        await Page.Locator("#regNombre").FillAsync(TestConstants.NewUserName);
        await Page.Locator("#regApellidos").FillAsync(TestConstants.NewUserLastName);
        await Page.Locator("#regEmail").FillAsync(uniqueEmail);
        await Page.Locator("#regPassword").FillAsync(TestConstants.NewUserPassword);
        await Page.Locator("#regConfirm").FillAsync(TestConstants.NewUserPassword);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Crear cuenta" }).ClickAsync();

        // Tras registro exitoso → auto-login → redirige al index
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$"));
    }

    // ══════════════════════════════════════════════════════════════
    // NAVEGACIÓN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task Register_TieneEnlaceALogin()
    {
        await GoToPage(TestConstants.RegisterPath);
        var loginLink = Page.Locator("a[href*='Login']").First;
        await Expect(loginLink).ToBeVisibleAsync();
    }
}
