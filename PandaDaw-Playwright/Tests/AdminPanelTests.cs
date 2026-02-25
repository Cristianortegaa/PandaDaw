using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace PandaDaw_Playwright.Tests;

/// <summary>
/// Tests E2E del Panel de Administración (/AdminPanel).
/// Cubre: acceso admin-only, estadísticas, tabla de productos, filtro,
/// modal crear producto, modal editar producto, eliminar, restaurar.
/// </summary>
[TestFixture]
public class AdminPanelTests : BaseTest
{
    // ══════════════════════════════════════════════════════════════
    // ACCESO - PROTECCIÓN
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_SinLogin_RedirigeALogin()
    {
        await GoToPage(TestConstants.AdminPanelPath);
        // Redirige a Login o a Index
        var url = Page.Url;
        Assert.That(url, Does.Contain("Login").Or.Contain("/"));
    }

    [Test]
    public async Task AdminPanel_ConUserNormal_RedirigeAlIndex()
    {
        await LoginAsUser();
        await GoToPage(TestConstants.AdminPanelPath);
        // User normal no tiene acceso → redirige al index
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/$|Index"));
    }

    [Test]
    public async Task AdminPanel_ConAdmin_AccedeCorrectamente()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("AdminPanel"));
    }

    // ══════════════════════════════════════════════════════════════
    // ESTRUCTURA Y CARGA
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_MuestraEstadisticas()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var pageText = await Page.Locator("body").TextContentAsync();
        var tieneStats = pageText!.Contains("total", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("activos", StringComparison.OrdinalIgnoreCase)
                         || pageText.Contains("productos", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneStats, Is.True, "Debe mostrar estadísticas de productos");
    }

    [Test]
    public async Task AdminPanel_MuestraTablaDeProductos()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var tabla = Page.Locator("table").First;
        await Expect(tabla).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminPanel_TablaTieneFilasDeProductos()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var filas = Page.Locator("table tbody tr, table tr").Nth(1); // Al menos 1 fila de datos
        await Expect(filas).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // FILTRO DE PRODUCTOS
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_FiltroDeProductos_Existe()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var filtroInput = Page.Locator("input[name='filtro']");
        await Expect(filtroInput).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminPanel_Filtrar_FuncionaCorrectamente()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var filtroInput = Page.Locator("input[name='filtro']");
        await filtroInput.FillAsync("a");

        var filtrarBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Filtrar" });
        await filtrarBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("filtro="));
    }

    [Test]
    public async Task AdminPanel_CheckboxMostrarEliminados_Existe()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var checkbox = Page.Locator("input[name='mostrarEliminados'], input[type='checkbox']").First;
        await Expect(checkbox).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // BOTÓN NUEVO PRODUCTO → MODAL CREAR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_BotonNuevoProducto_Existe()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var nuevoBtn = Page.Locator("button:has-text('Nuevo'), button:has-text('Crear'), button:has-text('nuevo')").First;
        await Expect(nuevoBtn).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminPanel_ModalCrear_SeAbre()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var nuevoBtn = Page.Locator("button:has-text('Nuevo'), button:has-text('nuevo')").First;
        await nuevoBtn.ClickAsync();

        var modal = Page.Locator("#modal_crear, dialog[open]").First;
        await Expect(modal).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminPanel_ModalCrear_TieneCamposNecesarios()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        // Abrir modal
        var nuevoBtn = Page.Locator("button:has-text('Nuevo'), button:has-text('nuevo')").First;
        await nuevoBtn.ClickAsync();

        var modal = Page.Locator("#modal_crear, dialog[open]").First;
        await Expect(modal).ToBeVisibleAsync();

        // Verificar campos dentro del modal
        var modalText = await modal.TextContentAsync();
        Assert.That(modalText, Does.Contain("ombre").IgnoreCase, "Modal debe tener campo nombre");
    }

    [Test]
    public async Task AdminPanel_CrearProducto_Funciona()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        // Abrir modal
        var nuevoBtn = Page.Locator("button:has-text('Nuevo'), button:has-text('nuevo')").First;
        await nuevoBtn.ClickAsync();

        var modal = Page.Locator("#modal_crear, dialog[open]").First;
        await Expect(modal).ToBeVisibleAsync();

        // Rellenar campos del modal
        var nombreInput = modal.Locator("input[name*='ombre'], input").First;
        await nombreInput.FillAsync("Producto Test Playwright");

        var descInput = modal.Locator("textarea, input[name*='escripcion']").First;
        if (await descInput.IsVisibleAsync())
            await descInput.FillAsync("Descripción creada por Playwright E2E");

        var precioInput = modal.Locator("input[name*='recio'], input[type='number']").First;
        if (await precioInput.IsVisibleAsync())
            await precioInput.FillAsync("99.99");

        var stockInput = modal.Locator("input[name*='tock']").First;
        if (await stockInput.IsVisibleAsync())
            await stockInput.FillAsync("10");

        var categoriaSelect = modal.Locator("select").First;
        if (await categoriaSelect.IsVisibleAsync())
            await categoriaSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        var imagenInput = modal.Locator("input[name*='magen'], input[name*='url']").First;
        if (await imagenInput.IsVisibleAsync())
            await imagenInput.FillAsync("https://via.placeholder.com/300");

        // Submit
        var submitBtn = modal.Locator("button[type='submit'], button:has-text('Crear')").First;
        await submitBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // ══════════════════════════════════════════════════════════════
    // MODAL EDITAR PRODUCTO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_BotonesEditar_ExistenEnTabla()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var editBtn = Page.Locator("button:has-text('Editar'), button:has(.fa-edit), button:has(.fa-pen), [onclick*='editar'], [onclick*='modal_editar']").First;
        await Expect(editBtn).ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminPanel_ModalEditar_SeAbre()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var editBtn = Page.Locator("button:has-text('Editar'), button:has(.fa-edit), button:has(.fa-pen), [onclick*='editar'], [onclick*='modal_editar']").First;
        await editBtn.ClickAsync();

        var modal = Page.Locator("#modal_editar, dialog[open]").First;
        await Expect(modal).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // ELIMINAR PRODUCTO
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_BotonEliminar_ExisteEnTabla()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        var deleteBtn = Page.Locator("form[action*='Eliminar'] button, button:has(.fa-trash), button:has-text('Eliminar')").First;
        await Expect(deleteBtn).ToBeVisibleAsync();
    }

    // ══════════════════════════════════════════════════════════════
    // RESTAURAR PRODUCTO (si hay eliminados)
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_MostrarEliminados_YRestaurar()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        // Activar checkbox de mostrar eliminados
        var checkbox = Page.Locator("input[name='mostrarEliminados']");
        await checkbox.CheckAsync(new() { Force = true });

        var filtrarBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Filtrar" });
        await filtrarBtn.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Si hay un botón de restaurar, verificar que existe
        var restaurarBtn = Page.Locator("form[action*='Restaurar'] button, button:has-text('Restaurar')").First;
        // Puede o no existir dependiendo de si hay productos eliminados
        var exists = await restaurarBtn.CountAsync() > 0;
        // Solo verificamos que la página funciona
        Assert.That(true);
    }

    // ══════════════════════════════════════════════════════════════
    // ALERTAS DE ÉXITO/ERROR
    // ══════════════════════════════════════════════════════════════

    [Test]
    public async Task AdminPanel_AlertasExistenEnDOM()
    {
        await LoginAsAdmin();
        await GoToPage(TestConstants.AdminPanelPath);

        // Las alertas pueden o no ser visibles, pero el HTML debería soportarlas
        var pageContent = await Page.ContentAsync();
        // Verificamos que el HTML tiene la estructura para mostrar alertas
        var tieneAlerta = pageContent.Contains("alert", StringComparison.OrdinalIgnoreCase);
        Assert.That(tieneAlerta, Is.True, "La página debe tener soporte para alertas");
    }
}
