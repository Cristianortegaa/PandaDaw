using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E de la página de Error (/Error).
/// </summary>
[TestFixture]
public class ErrorTests : BaseTest
{
    [Test]
    public async Task Error_PaginaSeRenderiza()
    {
        await GoToPage("/Error");
        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneContenido = pageText!.Contains("error", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("mal", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("salió", StringComparison.OrdinalIgnoreCase)
                             || pageText.Contains("Inicio", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneContenido, Is.True, "La página de error debe tener contenido informativo");
    }

    [Test]
    public async Task Error_TieneEnlaceDeVueltaAlInicio()
    {
        await GoToPage("/Error");
        var inicioLink = Page.Locator("a[href='/'], a[href*='Index'], a:has-text('Inicio'), a:has-text('inicio'), a:has-text('Volver')").First;
        await Expect(inicioLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task Error_RutaNoExistente_ManejaCorrectamente()
    {
        // Navegar a una ruta que no existe
        await GoToPage("/RutaQueNoExiste12345");
        // Debe mostrar error o redirigir, no crash
        var pageText = await Page.Locator("body").TextContentAsync();
        Assert.That(pageText, Is.Not.Null.And.Not.Empty, "Debe mostrar contenido, no un crash");
    }
}
