using System.Security.Claims;
using CSharpFunctionalExtensions;
using Moq;
using PandaBack.Dtos.Ventas;
using PandaBack.Errors;
using PandaBack.Models;
using PandaBack.RestController;
using PandaBack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class VentasControllerTest
    {
        private VentasController _controller;
        private Mock<IVentaService> _serviceFalso;
        private const string TestUserId = "test-user-id";

        [SetUp]
        public void PrepararTodo()
        {
            _serviceFalso = new Mock<IVentaService>();
            _controller = new VentasController(_serviceFalso.Object);

            // Simular usuario autenticado
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, TestUserId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ==========================================
        // 1. PRUEBAS DE OBTENER TODAS LAS VENTAS (ADMIN)
        // ==========================================

        [Test]
        public async Task GetAll_SiHayVentas_DebeDevolver200()
        {
            // PREPARAR
            var listaVentas = new List<VentaResponseDto>
            {
                new VentaResponseDto
                {
                    Id = 1, Total = 150, Estado = "Pendiente",
                    UsuarioId = TestUserId, UsuarioNombre = "Jorge",
                    Lineas = new List<LineaVentaResponseDto>()
                }
            };

            _serviceFalso.Setup(s => s.GetAllVentasAsync())
                .ReturnsAsync(Result.Success<IEnumerable<VentaResponseDto>, PandaError>(listaVentas));

            // ACTUAR
            var resultado = await _controller.GetAllAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAll_SiHayError_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetAllVentasAsync())
                .ReturnsAsync(Result.Failure<IEnumerable<VentaResponseDto>, PandaError>(new BadRequestError("Error interno")));

            // ACTUAR
            var resultado = await _controller.GetAllAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 2. PRUEBAS DE OBTENER MIS PEDIDOS
        // ==========================================

        [Test]
        public async Task GetMyOrders_DebeDevolver200ConLista()
        {
            // PREPARAR
            var listaVentas = new List<VentaResponseDto>
            {
                new VentaResponseDto { Id = 1, Total = 100, Estado = "Pendiente", UsuarioId = TestUserId }
            };

            _serviceFalso.Setup(s => s.GetVentasByUserAsync(TestUserId))
                .ReturnsAsync(Result.Success<IEnumerable<VentaResponseDto>, PandaError>(listaVentas));

            // ACTUAR
            var resultado = await _controller.GetMyOrdersAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyOrders_SiHayError_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetVentasByUserAsync(TestUserId))
                .ReturnsAsync(Result.Failure<IEnumerable<VentaResponseDto>, PandaError>(new BadRequestError("Error interno")));

            // ACTUAR
            var resultado = await _controller.GetMyOrdersAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 3. PRUEBAS DE OBTENER VENTA POR ID
        // ==========================================

        [Test]
        public async Task GetById_SiExiste_DebeDevolver200()
        {
            // PREPARAR
            var ventaResp = new VentaResponseDto
            {
                Id = 1, Total = 150, Estado = "Pendiente",
                UsuarioId = TestUserId, UsuarioNombre = "Jorge"
            };

            _serviceFalso.Setup(s => s.GetVentaByIdAsync(1))
                .ReturnsAsync(Result.Success<VentaResponseDto, PandaError>(ventaResp));

            // ACTUAR
            var resultado = await _controller.GetByIdAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetById_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetVentaByIdAsync(99))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new NotFoundError("Venta no encontrada")));

            // ACTUAR
            var resultado = await _controller.GetByIdAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetById_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetVentaByIdAsync(1))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.GetByIdAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 4. PRUEBAS DE CREAR VENTA DESDE CARRITO
        // ==========================================

        [Test]
        public async Task CreateFromCarrito_ConCarritoValido_DebeDevolver201()
        {
            // PREPARAR
            var ventaResp = new VentaResponseDto
            {
                Id = 1, Total = 200, Estado = "Pendiente",
                UsuarioId = TestUserId,
                Lineas = new List<LineaVentaResponseDto>
                {
                    new LineaVentaResponseDto { ProductoId = 5, Cantidad = 2, PrecioUnitario = 100, Subtotal = 200 }
                }
            };

            _serviceFalso.Setup(s => s.CreateVentaFromCarritoAsync(TestUserId))
                .ReturnsAsync(Result.Success<VentaResponseDto, PandaError>(ventaResp));

            // ACTUAR
            var resultado = await _controller.CreateFromCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<CreatedResult>());
        }

        [Test]
        public async Task CreateFromCarrito_SiCarritoVacio_DebeDevolver400()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.CreateVentaFromCarritoAsync(TestUserId))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new CarritoVacioError("El carrito está vacío")));

            // ACTUAR
            var resultado = await _controller.CreateFromCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateFromCarrito_SiStockInsuficiente_DebeDevolver400()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.CreateVentaFromCarritoAsync(TestUserId))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new StockInsuficienteError("Stock insuficiente")));

            // ACTUAR
            var resultado = await _controller.CreateFromCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateFromCarrito_SiProductoNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.CreateVentaFromCarritoAsync(TestUserId))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.CreateFromCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateFromCarrito_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.CreateVentaFromCarritoAsync(TestUserId))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.CreateFromCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 5. PRUEBAS DE ACTUALIZAR ESTADO DE VENTA (ADMIN)
        // ==========================================

        [Test]
        public async Task UpdateEstado_ConEstadoValido_DebeDevolver200()
        {
            // PREPARAR
            var ventaResp = new VentaResponseDto
            {
                Id = 1, Total = 150, Estado = "Procesando", UsuarioId = TestUserId
            };

            _serviceFalso.Setup(s => s.UpdateEstadoVentaAsync(1, EstadoPedido.Procesando))
                .ReturnsAsync(Result.Success<VentaResponseDto, PandaError>(ventaResp));

            // ACTUAR
            var resultado = await _controller.UpdateEstadoAsync(1, EstadoPedido.Procesando);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateEstado_SiVentaNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.UpdateEstadoVentaAsync(99, EstadoPedido.Enviado))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new NotFoundError("Venta no encontrada")));

            // ACTUAR
            var resultado = await _controller.UpdateEstadoAsync(99, EstadoPedido.Enviado);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateEstado_SiCambioNoPermitido_DebeDevolver400()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.UpdateEstadoVentaAsync(1, EstadoPedido.Pendiente))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new OperacionNoPermitidaError("Cambio de estado no permitido")));

            // ACTUAR
            var resultado = await _controller.UpdateEstadoAsync(1, EstadoPedido.Pendiente);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateEstado_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.UpdateEstadoVentaAsync(1, EstadoPedido.Enviado))
                .ReturnsAsync(Result.Failure<VentaResponseDto, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.UpdateEstadoAsync(1, EstadoPedido.Enviado);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }
    }
}
