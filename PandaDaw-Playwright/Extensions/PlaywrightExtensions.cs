using Microsoft.Playwright;

namespace PandaDaw_Playwright;

/// <summary>
/// Funciones de extensión para Playwright.
/// Permiten sintaxis fluida con TestId y desacoplar tests de CSS/estructura DOM.
/// Si cambian los estilos o la estructura, solo hay que tocar estos helpers.
/// </summary>
public static class PlaywrightExtensions
{
    // ── Localizadores por data-testid ──
    public static ILocator TestId(this IPage page, string id) => page.GetByTestId(id);
    public static ILocator TestId(this ILocator locator, string id) => locator.GetByTestId(id);

    // ── Localizadores por ID HTML ──
    public static ILocator ById(this IPage page, string id) => page.Locator($"#{id}");
    public static ILocator ById(this ILocator locator, string id) => locator.Locator($"#{id}");

    // ── Localizadores por selector CSS resiliente ──
    public static ILocator ByCss(this IPage page, string css) => page.Locator(css);
    public static ILocator ByCss(this ILocator locator, string css) => locator.Locator(css);

    // ── Localizadores por atributo name (formularios) ──
    public static ILocator ByName(this IPage page, string name) => page.Locator($"[name='{name}']");
    public static ILocator ByName(this ILocator locator, string name) => locator.Locator($"[name='{name}']");

    // ── Localizadores por placeholder ──
    public static ILocator ByPlaceholder(this IPage page, string text) => page.GetByPlaceholder(text);

    // ── Localizador de formulario por handler de Razor Pages (asp-page-handler) ──
    public static ILocator FormByHandler(this IPage page, string handler) =>
        page.Locator($"form[action*='handler={handler}'], form[action*='Handler={handler}']");

    // ── Submit de un formulario ──
    public static ILocator SubmitButton(this ILocator form) =>
        form.Locator("button[type='submit'], input[type='submit']");

    // ── Helpers para inputs dentro de locators ──
    public static ILocator Input(this ILocator locator, string name) =>
        locator.Locator($"input[name='{name}'], textarea[name='{name}'], select[name='{name}']");

    // ── Helper para links por texto parcial ──
    public static ILocator LinkByText(this IPage page, string text) =>
        page.GetByRole(AriaRole.Link, new() { Name = text });

    // ── Helper para botones por texto parcial ──
    public static ILocator ButtonByText(this IPage page, string text) =>
        page.GetByRole(AriaRole.Button, new() { Name = text });

    // ── Helper para heading por texto ──
    public static ILocator HeadingByText(this IPage page, string text) =>
        page.GetByRole(AriaRole.Heading, new() { Name = text });
}
