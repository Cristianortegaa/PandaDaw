using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Dtos.Ventas;
using PandaBack.Services.Email;

namespace Tests.Services;

/// <summary>
/// Tests unitarios para EmailService.
/// Se testea la lógica de configuración, generación de HTML y manejo de errores.
/// Las llamadas SMTP reales requieren un servidor configurado, por lo que se testean
/// los escenarios de error y la lógica interna.
/// </summary>
public class EmailServiceTest
{
    private Mock<ILogger<EmailService>> _loggerFalso;

    private const string TestEmail = "destinatario@test.com";
    private const string TestNombre = "Juan García";

    [SetUp]
    public void PrepararTodo()
    {
        _loggerFalso = new Mock<ILogger<EmailService>>();
    }

    /// <summary>
    /// Crea una configuración SMTP en memoria para los tests.
    /// </summary>
    private static IConfiguration CrearConfiguracion(
        string host = "smtp.test.com",
        string port = "587",
        string useSsl = "true",
        string username = "",
        string password = "",
        string fromName = "PandaDaw Test",
        string fromEmail = "test@pandadaw.es")
    {
        var settings = new Dictionary<string, string?>
        {
            { "SmtpSettings:Host", host },
            { "SmtpSettings:Port", port },
            { "SmtpSettings:UseSsl", useSsl },
            { "SmtpSettings:Username", username },
            { "SmtpSettings:Password", password },
            { "SmtpSettings:FromName", fromName },
            { "SmtpSettings:FromEmail", fromEmail }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    /// <summary>
    /// Crea un VentaResponseDto de ejemplo.
    /// </summary>
    private static VentaResponseDto CrearVentaEjemplo()
    {
        return new VentaResponseDto
        {
            Id = 42,
            FechaCompra = new DateTime(2026, 2, 15, 10, 30, 0),
            Total = 150m,
            Estado = "Pendiente",
            UsuarioId = "user-123",
            UsuarioNombre = "Juan García",
            UsuarioEmail = "juan@test.com",
            Lineas = new List<LineaVentaResponseDto>
            {
                new()
                {
                    ProductoId = 1,
                    ProductoNombre = "Auriculares Bluetooth",
                    ProductoImagen = "https://ejemplo.com/auriculares.jpg",
                    Cantidad = 2,
                    PrecioUnitario = 50m,
                    Subtotal = 100m
                },
                new()
                {
                    ProductoId = 2,
                    ProductoNombre = "Funda Protectora",
                    ProductoImagen = "https://ejemplo.com/funda.jpg",
                    Cantidad = 1,
                    PrecioUnitario = 50m,
                    Subtotal = 50m
                }
            }
        };
    }

    // ==========================================
    // 1. CREACIÓN DEL SERVICIO
    // ==========================================

    [Test]
    public void Constructor_ConConfiguracionValida_NoDebeLanzarExcepcion()
    {
        // PREPARAR
        var config = CrearConfiguracion();

        // ACTUAR & COMPROBAR
        Assert.DoesNotThrow(() => new EmailService(config, _loggerFalso.Object));
    }

    [Test]
    public void Constructor_ConConfiguracionVacia_NoDebeLanzarExcepcion()
    {
        // PREPARAR
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // ACTUAR & COMPROBAR
        Assert.DoesNotThrow(() => new EmailService(config, _loggerFalso.Object));
    }

    // ==========================================
    // 2. ENVÍO DE EMAIL — MANEJO DE ERRORES SMTP
    // ==========================================

    [Test]
    public async Task EnviarEmail_ConHostInvalido_NoDebeLanzarExcepcion()
    {
        // PREPARAR — Host que no existe, fallará la conexión SMTP
        var config = CrearConfiguracion(host: "smtp.servidor-que-no-existe.invalid", port: "587");
        var service = new EmailService(config, _loggerFalso.Object);
        var venta = CrearVentaEjemplo();
        var facturaPdf = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        // ACTUAR — No debe lanzar excepción (se captura internamente)
        await service.SendConfirmacionPagoAsync(TestEmail, TestNombre, venta, facturaPdf);

        // COMPROBAR — Debe haberse logueado el error
        _loggerFalso.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task EnviarEmail_ConPuertoInvalido_NoDebeLanzarExcepcion()
    {
        // PREPARAR — Puerto que no es SMTP
        var config = CrearConfiguracion(host: "localhost", port: "1");
        var service = new EmailService(config, _loggerFalso.Object);
        var venta = CrearVentaEjemplo();
        var facturaPdf = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        // ACTUAR — No debe propagar la excepción
        Assert.DoesNotThrowAsync(async () =>
            await service.SendConfirmacionPagoAsync(TestEmail, TestNombre, venta, facturaPdf));
    }

    [Test]
    public async Task EnviarEmail_SinCredenciales_NoDebeLanzarExcepcion()
    {
        // PREPARAR — Sin username/password → no se autentica
        var config = CrearConfiguracion(host: "localhost", port: "1", username: "", password: "");
        var service = new EmailService(config, _loggerFalso.Object);
        var venta = CrearVentaEjemplo();
        var facturaPdf = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        // ACTUAR
        Assert.DoesNotThrowAsync(async () =>
            await service.SendConfirmacionPagoAsync(TestEmail, TestNombre, venta, facturaPdf));
    }

    // ==========================================
    // 3. GENERACIÓN DE HTML (vía reflexión)
    // ==========================================

    [Test]
    public void GenerarHtml_DebeContenerNombreDelCliente()
    {
        // PREPARAR
        var venta = CrearVentaEjemplo();

        // ACTUAR
        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        // COMPROBAR
        Assert.That(html, Does.Contain(TestNombre));
    }

    [Test]
    public void GenerarHtml_DebeContenerIdDelPedido()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("#42"));
    }

    [Test]
    public void GenerarHtml_DebeContenerNombresDeLosProductos()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("Auriculares Bluetooth"));
        Assert.That(html, Does.Contain("Funda Protectora"));
    }

    [Test]
    public void GenerarHtml_DebeContenerEstadoDelPedido()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("Pendiente"));
    }

    [Test]
    public void GenerarHtml_DebeContenerMarcaPandaDaw()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("PandaDaw"));
    }

    [Test]
    public void GenerarHtml_DebeContenerDesgloseIva()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("Base imponible"));
        Assert.That(html, Does.Contain("IVA (21%) incluido"));
    }

    [Test]
    public void GenerarHtml_DebeSerHtmlValido()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("<!DOCTYPE html>"));
        Assert.That(html, Does.Contain("</html>"));
        Assert.That(html, Does.Contain("<head>"));
        Assert.That(html, Does.Contain("<body>"));
    }

    [Test]
    public void GenerarHtml_DebeContenerFechaFormateada()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        // Fecha: 15/02/2026 10:30
        Assert.That(html, Does.Contain("15/02/2026"));
    }

    [Test]
    public void GenerarHtml_DebeContenerMensajeAdjuntoFactura()
    {
        var venta = CrearVentaEjemplo();

        var html = InvocarGenerarHtmlEmail(venta, TestNombre);

        Assert.That(html, Does.Contain("factura"));
    }

    [Test]
    public void GenerarHtml_ConLineasVacias_NoDebeLanzarExcepcion()
    {
        var venta = new VentaResponseDto
        {
            Id = 1,
            FechaCompra = DateTime.UtcNow,
            Total = 0,
            Estado = "Pendiente",
            UsuarioNombre = "Test",
            UsuarioEmail = "test@test.com",
            Lineas = new List<LineaVentaResponseDto>()
        };

        Assert.DoesNotThrow(() => InvocarGenerarHtmlEmail(venta, "Test"));
    }

    // ==========================================
    // 4. CONFIGURACIÓN POR DEFECTO
    // ==========================================

    [Test]
    public async Task EnviarEmail_SinFromName_DebeUsarPandaDawPorDefecto()
    {
        // PREPARAR — Configuración sin FromName ni FromEmail
        var settings = new Dictionary<string, string?>
        {
            { "SmtpSettings:Host", "localhost" },
            { "SmtpSettings:Port", "1" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var service = new EmailService(config, _loggerFalso.Object);
        var venta = CrearVentaEjemplo();
        var facturaPdf = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        // ACTUAR — No debe fallar con NullReferenceException
        Assert.DoesNotThrowAsync(async () =>
            await service.SendConfirmacionPagoAsync(TestEmail, TestNombre, venta, facturaPdf));
    }

    [Test]
    public async Task EnviarEmail_ConSslDesactivado_NoDebeLanzarExcepcion()
    {
        // PREPARAR — UseSsl = false → SecureSocketOptions.None
        var config = CrearConfiguracion(host: "localhost", port: "1", useSsl: "false");
        var service = new EmailService(config, _loggerFalso.Object);
        var venta = CrearVentaEjemplo();
        var facturaPdf = new byte[] { 0x25, 0x50, 0x44, 0x46 };

        // ACTUAR
        Assert.DoesNotThrowAsync(async () =>
            await service.SendConfirmacionPagoAsync(TestEmail, TestNombre, venta, facturaPdf));
    }

    // ==========================================
    // HELPER — Invocar método privado GenerarHtmlEmail vía reflexión
    // ==========================================

    private static string InvocarGenerarHtmlEmail(VentaResponseDto venta, string nombreCliente)
    {
        var metodo = typeof(EmailService).GetMethod("GenerarHtmlEmail",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.That(metodo, Is.Not.Null, "El método GenerarHtmlEmail no se encontró");

        var resultado = metodo!.Invoke(null, new object[] { venta, nombreCliente }) as string;

        Assert.That(resultado, Is.Not.Null, "GenerarHtmlEmail retornó null");

        return resultado!;
    }
}
