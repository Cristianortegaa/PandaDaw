namespace PandaDaw_Playwright;

/// <summary>
/// Constantes globales del proyecto de tests E2E.
/// Centraliza URLs, credenciales seed y rutas para facilitar mantenimiento.
/// </summary>
public static class TestConstants
{
    // ── URLs base ──
    // Cambiar según entorno: Docker → http://localhost:5062 | Dev local → http://localhost:5062
    public const string BaseUrl = "http://localhost:5062";

    // ── Credenciales de usuario seed (DataSeeder) ──
    public const string AdminEmail = "admin@pandadaw.com";
    public const string AdminPassword = "Admin123!";
    public const string AdminName = "Admin";

    public const string UserEmail = "usuario1@pandadaw.com";
    public const string UserPassword = "Usuario123!";
    public const string UserName = "Usuario";

    public const string User2Email = "usuario2@pandadaw.com";
    public const string User2Password = "Usuario123!";

    // ── Credenciales de registro para tests ──
    public const string NewUserName = "TestPlaywright";
    public const string NewUserLastName = "Automatizado";
    public const string NewUserEmail = "test.playwright@pandadaw.com";
    public const string NewUserPassword = "TestPw123!";

    // ── Rutas de la aplicación ──
    public const string LoginPath = "/Login";
    public const string RegisterPath = "/Register";
    public const string IndexPath = "/";
    public const string CarritoPath = "/Carrito";
    public const string FavoritosPath = "/Favoritos";
    public const string PedidosPath = "/Pedidos";
    public const string PagoPath = "/Pago";
    public const string AdminPanelPath = "/AdminPanel";
    public const string LogoutPath = "/Logout";
    public const string DetallePath = "/Detalle";

    // ── Categorías disponibles ──
    public static readonly string[] Categorias = ["Smartphones", "Audio", "Laptops", "Gaming", "Imagen"];

    // ── Carpeta de resultados ──
    public const string ResultsDir = "results";
    public const string ScreenshotsDir = "results/screenshots";
    public const string VideosDir = "results/videos";
    public const string TracesDir = "results/traces";
}
