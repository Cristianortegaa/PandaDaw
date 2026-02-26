using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Errors;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Services.Stripe;
using Stripe;
using Stripe.Checkout;

namespace Tests.Services;

/// <summary>
/// Tests unitarios para StripeService.
/// Se testea la lógica de validación previa a Stripe (carrito vacío, URLs inválidas, imágenes).
/// Las llamadas reales a la API de Stripe se evitan testeando solo la lógica interna.
/// </summary>
public class StripeServiceTest
{
    private StripeService _service;
    private Mock<ICarritoRepository> _repoCarritoFalso;
    private Mock<ILogger<StripeService>> _loggerFalso;
    private IConfiguration _configuration;

    private const string TestUserId = "test-user-id";
    private const string SuccessUrl = "https://localhost/pago-exitoso";
    private const string CancelUrl = "https://localhost/carrito";

    [SetUp]
    public void PrepararTodo()
    {
        _repoCarritoFalso = new Mock<ICarritoRepository>();
        _loggerFalso = new Mock<ILogger<StripeService>>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Stripe:SecretKey", "sk_test_fake_key_para_tests_unitarios_1234567890" },
            { "Stripe:WebhookSecret", "whsec_test" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new StripeService(_repoCarritoFalso.Object, _loggerFalso.Object, _configuration);
    }

    // ==========================================
    // 1. TESTS DE CARRITO VACÍO
    // ==========================================

    [Test]
    public async Task CrearSesion_SinCarrito_DebeFallarConCarritoVacio()
    {
        // PREPARAR
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito?)null);

        // ACTUAR
        var resultado = await _service.CreateCheckoutSessionAsync(TestUserId, SuccessUrl, CancelUrl);

        // COMPROBAR
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<CarritoVacioError>());
    }

    [Test]
    public async Task CrearSesion_ConCarritoVacio_DebeFallarConCarritoVacio()
    {
        // PREPARAR
        var carrito = new Carrito
        {
            Id = 1,
            UserId = TestUserId,
            LineasCarrito = new List<LineaCarrito>()
        };
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carrito);

        // ACTUAR
        var resultado = await _service.CreateCheckoutSessionAsync(TestUserId, SuccessUrl, CancelUrl);

        // COMPROBAR
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<CarritoVacioError>());
    }

    // ==========================================
    // 2. TESTS DE VALIDACIÓN DE URLs
    // ==========================================

    [Test]
    public async Task CrearSesion_ConUrlNoAbsoluta_DebeFallarConBadRequest()
    {
        // PREPARAR
        var producto = new Producto { Id = 1, Nombre = "Test", Precio = 10, Stock = 5 };
        var carrito = new Carrito
        {
            Id = 1,
            UserId = TestUserId,
            LineasCarrito = new List<LineaCarrito>
            {
                new LineaCarrito { ProductoId = 1, Cantidad = 1, Producto = producto }
            }
        };
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carrito);

        // ACTUAR — URL relativa (no absoluta)
        var resultado = await _service.CreateCheckoutSessionAsync(TestUserId, "/pago-exitoso", CancelUrl);

        // COMPROBAR
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        Assert.That(resultado.Error.Message, Does.Contain("absolutas"));
    }

    [Test]
    public async Task CrearSesion_ConCancelUrlNoAbsoluta_DebeFallarConBadRequest()
    {
        // PREPARAR
        var producto = new Producto { Id = 1, Nombre = "Test", Precio = 10, Stock = 5 };
        var carrito = new Carrito
        {
            Id = 1,
            UserId = TestUserId,
            LineasCarrito = new List<LineaCarrito>
            {
                new LineaCarrito { ProductoId = 1, Cantidad = 1, Producto = producto }
            }
        };
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carrito);

        // ACTUAR — Cancel URL relativa
        var resultado = await _service.CreateCheckoutSessionAsync(TestUserId, SuccessUrl, "/carrito");

        // COMPROBAR
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
    }

    // ==========================================
    // 3. TESTS DE VALIDACIÓN DE IMÁGENES (método privado vía reflexión)
    // ==========================================

    [Test]
    public void ValidarUrlImagen_ConUrlAbsoluta_DebeRetornarLista()
    {
        // ACTUAR — usar reflexión para acceder al método privado
        var metodo = typeof(StripeService).GetMethod("ValidarUrlImagen",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var resultado = metodo?.Invoke(_service, new object?[] { "https://ejemplo.com/imagen.jpg" }) as List<string>;

        // COMPROBAR
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado, Has.Count.EqualTo(1));
        Assert.That(resultado![0], Is.EqualTo("https://ejemplo.com/imagen.jpg"));
    }

    [Test]
    public void ValidarUrlImagen_ConUrlRelativa_DebeRetornarNull()
    {
        var metodo = typeof(StripeService).GetMethod("ValidarUrlImagen",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var resultado = metodo?.Invoke(_service, new object?[] { "/img/productos/foto.jpg" });

        Assert.That(resultado, Is.Null);
    }

    [Test]
    public void ValidarUrlImagen_ConUrlNula_DebeRetornarNull()
    {
        var metodo = typeof(StripeService).GetMethod("ValidarUrlImagen",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var resultado = metodo?.Invoke(_service, new object?[] { null });

        Assert.That(resultado, Is.Null);
    }

    [Test]
    public void ValidarUrlImagen_ConUrlVacia_DebeRetornarNull()
    {
        var metodo = typeof(StripeService).GetMethod("ValidarUrlImagen",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var resultado = metodo?.Invoke(_service, new object?[] { "" });

        Assert.That(resultado, Is.Null);
    }

    // ==========================================
    // 4. TESTS DE CREACIÓN DE SESIÓN (con API de Stripe real → se espera StripeException por key fake)
    // ==========================================

    [Test]
    public async Task CrearSesion_ConSuccessUrlConQueryParams_DebeCubrirRamaAmpersand()
    {
        // PREPARAR — successUrl ya contiene "?" → se usa "&session_id=" en vez de "?session_id="
        var producto = new Producto { Id = 1, Nombre = "Test", Precio = 10, Stock = 5, Imagen = "https://ejemplo.com/img.jpg" };
        var carrito = new Carrito
        {
            Id = 1,
            UserId = TestUserId,
            LineasCarrito = new List<LineaCarrito>
            {
                new LineaCarrito { ProductoId = 1, Cantidad = 1, Producto = producto }
            }
        };
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carrito);

        // ACTUAR — successUrl con "?" → ejercita la rama true del ternario (línea 57)
        var resultado = await _service.CreateCheckoutSessionAsync(
            TestUserId,
            "https://localhost/pago?orden=123",
            CancelUrl);

        // COMPROBAR — Falla por key falsa, pero la rama del "?" se ejercitó
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
    }

    [Test]
    public async Task CrearSesion_ConDatosValidos_PeroKeyFake_DebeFallarConBadRequest()
    {
        // PREPARAR — datos válidos pero la key de Stripe es falsa
        var producto = new Producto { Id = 1, Nombre = "Auriculares", Precio = 49.99m, Stock = 10, Imagen = "https://ejemplo.com/img.jpg" };
        var carrito = new Carrito
        {
            Id = 1,
            UserId = TestUserId,
            LineasCarrito = new List<LineaCarrito>
            {
                new LineaCarrito { ProductoId = 1, Cantidad = 2, Producto = producto }
            }
        };
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carrito);

        // ACTUAR
        var resultado = await _service.CreateCheckoutSessionAsync(TestUserId, SuccessUrl, CancelUrl);

        // COMPROBAR — La API de Stripe rechaza la key falsa
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        Assert.That(resultado.Error.Message, Does.Contain("Stripe"));
    }

    [Test]
    public async Task CrearSesion_VerificaQueSeObtieneElCarritoDelUsuario()
    {
        // PREPARAR
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito?)null);

        // ACTUAR
        await _service.CreateCheckoutSessionAsync(TestUserId, SuccessUrl, CancelUrl);

        // COMPROBAR — Verificar que se llamó al repositorio con el userId correcto
        _repoCarritoFalso.Verify(r => r.GetByUserIdAsync(TestUserId), Times.Once);
    }

    // ==========================================
    // 5. TESTS DE GetSessionPaymentStatusAsync
    // ==========================================

    [Test]
    public async Task ObtenerEstadoPago_ConSessionIdInvalido_DebeFallarConBadRequest()
    {
        // ACTUAR — Session ID inexistente → StripeException
        var resultado = await _service.GetSessionPaymentStatusAsync("cs_test_inexistente");

        // COMPROBAR
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
    }

    [Test]
    public async Task ObtenerEstadoPago_ConSessionIdVacio_DebeFallarConBadRequest()
    {
        // ACTUAR
        var resultado = await _service.GetSessionPaymentStatusAsync("");

        // COMPROBAR
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
    }

    // ==========================================
    // 6. TESTS CON STRIPE MOCKEADO (ruta de éxito)
    // ==========================================

    [Test]
    public async Task CrearSesion_ConApiMockeada_DebeRetornarUrlExitosa()
    {
        // PREPARAR — Carrito válido
        var producto = new Producto { Id = 1, Nombre = "Auriculares", Precio = 49.99m, Stock = 10, Imagen = "https://ejemplo.com/img.jpg" };
        var carrito = new Carrito
        {
            Id = 1,
            UserId = TestUserId,
            LineasCarrito = new List<LineaCarrito>
            {
                new LineaCarrito { ProductoId = 1, Cantidad = 1, Producto = producto }
            }
        };
        _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carrito);

        // Mockear el cliente de Stripe para devolver una sesión de éxito
        var mockClient = new Mock<IStripeClient>();
        mockClient
            .Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session { Url = "https://checkout.stripe.com/test-session" });

        var savedClient = StripeConfiguration.StripeClient;
        StripeConfiguration.StripeClient = mockClient.Object;
        try
        {
            // ACTUAR
            var resultado = await _service.CreateCheckoutSessionAsync(TestUserId, SuccessUrl, CancelUrl);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value, Is.EqualTo("https://checkout.stripe.com/test-session"));
        }
        finally
        {
            StripeConfiguration.StripeClient = savedClient;
        }
    }

    [Test]
    public async Task ObtenerEstadoPago_ConApiMockeada_DebeRetornarEstado()
    {
        // PREPARAR — Mockear el cliente de Stripe
        var mockClient = new Mock<IStripeClient>();
        mockClient
            .Setup(c => c.RequestAsync<Session>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<BaseOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session { PaymentStatus = "paid" });

        var savedClient = StripeConfiguration.StripeClient;
        StripeConfiguration.StripeClient = mockClient.Object;
        try
        {
            // ACTUAR
            var resultado = await _service.GetSessionPaymentStatusAsync("cs_test_456");

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value, Is.EqualTo("paid"));
        }
        finally
        {
            StripeConfiguration.StripeClient = savedClient;
        }
    }
}
