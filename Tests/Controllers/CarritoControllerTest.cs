using System.Security.Claims;
using CSharpFunctionalExtensions;
using Moq;
using PandaBack.Dtos.Carrito;
using PandaBack.Errors;
using PandaBack.RestController;
using PandaBack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class CarritoControllerTest
    {
        private CarritoController _controller;
        private Mock<ICarritoService> _serviceFalso;
        private const string TestUserId = "test-user-id";

        [SetUp]
        public void PrepararTodo()
        {
            _serviceFalso = new Mock<ICarritoService>();
            _controller = new CarritoController(_serviceFalso.Object);

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
        // 1. PRUEBAS DE OBTENER CARRITO
        // ==========================================

        [Test]
        public async Task GetCarrito_SiExiste_DebeDevolver200()
        {
            // PREPARAR
            var carritoDto = new CarritoDto
            {
                Id = 1,
                UsuarioId = TestUserId,
                Lineas = new List<LineaCarritoDto>(),
                Total = 0,
                TotalItems = 0
            };

            _serviceFalso.Setup(s => s.GetCarritoByUserIdAsync(TestUserId))
                .ReturnsAsync(Result.Success<CarritoDto, PandaError>(carritoDto));

            // ACTUAR
            var resultado = await _controller.GetCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetCarrito_SiHayError_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetCarritoByUserIdAsync(TestUserId))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new BadRequestError("Error interno")));

            // ACTUAR
            var resultado = await _controller.GetCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 2. PRUEBAS DE AÑADIR LÍNEA AL CARRITO
        // ==========================================

        [Test]
        public async Task AddLinea_ConDatosCorrectos_DebeDevolver200()
        {
            // PREPARAR
            var dto = new AddLineaCarritoRequestDto { ProductoId = 5, Cantidad = 2 };
            var carritoDto = new CarritoDto
            {
                Id = 1,
                UsuarioId = TestUserId,
                Lineas = new List<LineaCarritoDto>
                {
                    new LineaCarritoDto { ProductoId = 5, Cantidad = 2, PrecioUnitario = 50, Subtotal = 100 }
                },
                Total = 100,
                TotalItems = 2
            };

            _serviceFalso.Setup(s => s.AddLineaCarritoAsync(TestUserId, 5, 2))
                .ReturnsAsync(Result.Success<CarritoDto, PandaError>(carritoDto));

            // ACTUAR
            var resultado = await _controller.AddLineaAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddLinea_SiProductoNoExiste_DebeDevolver404()
        {
            // PREPARAR
            var dto = new AddLineaCarritoRequestDto { ProductoId = 99, Cantidad = 1 };

            _serviceFalso.Setup(s => s.AddLineaCarritoAsync(TestUserId, 99, 1))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.AddLineaAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AddLinea_SiCantidadInvalida_DebeDevolver400()
        {
            // PREPARAR
            var dto = new AddLineaCarritoRequestDto { ProductoId = 5, Cantidad = 0 };

            _serviceFalso.Setup(s => s.AddLineaCarritoAsync(TestUserId, 5, 0))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new BadRequestError("Cantidad inválida")));

            // ACTUAR
            var resultado = await _controller.AddLineaAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddLinea_SiStockInsuficiente_DebeDevolver400()
        {
            // PREPARAR
            var dto = new AddLineaCarritoRequestDto { ProductoId = 5, Cantidad = 999 };

            _serviceFalso.Setup(s => s.AddLineaCarritoAsync(TestUserId, 5, 999))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new StockInsuficienteError("Stock insuficiente")));

            // ACTUAR
            var resultado = await _controller.AddLineaAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddLinea_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            var dto = new AddLineaCarritoRequestDto { ProductoId = 5, Cantidad = 1 };

            _serviceFalso.Setup(s => s.AddLineaCarritoAsync(TestUserId, 5, 1))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.AddLineaAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 3. PRUEBAS DE ACTUALIZAR CANTIDAD DE LÍNEA
        // ==========================================

        [Test]
        public async Task UpdateLineaCantidad_ConDatosCorrectos_DebeDevolver200()
        {
            // PREPARAR
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = 3 };
            var carritoDto = new CarritoDto { Id = 1, UsuarioId = TestUserId, Total = 150 };

            _serviceFalso.Setup(s => s.UpdateLineaCantidadAsync(TestUserId, 5, 3))
                .ReturnsAsync(Result.Success<CarritoDto, PandaError>(carritoDto));

            // ACTUAR
            var resultado = await _controller.UpdateLineaCantidadAsync(5, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateLineaCantidad_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = 3 };

            _serviceFalso.Setup(s => s.UpdateLineaCantidadAsync(TestUserId, 99, 3))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new NotFoundError("Línea no encontrada")));

            // ACTUAR
            var resultado = await _controller.UpdateLineaCantidadAsync(99, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateLineaCantidad_SiCantidadInvalida_DebeDevolver400()
        {
            // PREPARAR
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = -1 };

            _serviceFalso.Setup(s => s.UpdateLineaCantidadAsync(TestUserId, 5, -1))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new BadRequestError("Cantidad inválida")));

            // ACTUAR
            var resultado = await _controller.UpdateLineaCantidadAsync(5, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateLineaCantidad_SiStockInsuficiente_DebeDevolver400()
        {
            // PREPARAR
            var dto = new UpdateLineaCarritoRequestDto { Cantidad = 999 };

            _serviceFalso.Setup(s => s.UpdateLineaCantidadAsync(TestUserId, 5, 999))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new StockInsuficienteError("Stock insuficiente")));

            // ACTUAR
            var resultado = await _controller.UpdateLineaCantidadAsync(5, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ==========================================
        // 4. PRUEBAS DE ELIMINAR LÍNEA DEL CARRITO
        // ==========================================

        [Test]
        public async Task RemoveLinea_SiExiste_DebeDevolver200()
        {
            // PREPARAR
            var carritoDto = new CarritoDto { Id = 1, UsuarioId = TestUserId, Total = 0, TotalItems = 0 };

            _serviceFalso.Setup(s => s.RemoveLineaCarritoAsync(TestUserId, 5))
                .ReturnsAsync(Result.Success<CarritoDto, PandaError>(carritoDto));

            // ACTUAR
            var resultado = await _controller.RemoveLineaAsync(5);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RemoveLinea_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.RemoveLineaCarritoAsync(TestUserId, 99))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new NotFoundError("Línea no encontrada")));

            // ACTUAR
            var resultado = await _controller.RemoveLineaAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task RemoveLinea_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.RemoveLineaCarritoAsync(TestUserId, 5))
                .ReturnsAsync(Result.Failure<CarritoDto, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.RemoveLineaAsync(5);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 5. PRUEBAS DE VACIAR CARRITO
        // ==========================================

        [Test]
        public async Task VaciarCarrito_SiExiste_DebeDevolver204()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.VaciarCarritoAsync(TestUserId))
                .ReturnsAsync(UnitResult.Success<PandaError>());

            // ACTUAR
            var resultado = await _controller.VaciarCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task VaciarCarrito_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.VaciarCarritoAsync(TestUserId))
                .ReturnsAsync(UnitResult.Failure(new NotFoundError("Carrito no encontrado") as PandaError));

            // ACTUAR
            var resultado = await _controller.VaciarCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task VaciarCarrito_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.VaciarCarritoAsync(TestUserId))
                .ReturnsAsync(UnitResult.Failure(new ConflictError("Error inesperado") as PandaError));

            // ACTUAR
            var resultado = await _controller.VaciarCarritoAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }
    }
}
