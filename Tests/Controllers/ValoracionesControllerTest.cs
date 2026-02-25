using System.Security.Claims;
using CSharpFunctionalExtensions;
using Moq;
using PandaBack.Dtos.Valoraciones;
using PandaBack.Errors;
using PandaBack.RestController;
using PandaBack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class ValoracionesControllerTest
    {
        private ValoracionesController _controller;
        private Mock<IValoracionService> _serviceFalso;
        private const string TestUserId = "test-user-id";

        [SetUp]
        public void PrepararTodo()
        {
            _serviceFalso = new Mock<IValoracionService>();
            _controller = new ValoracionesController(_serviceFalso.Object);

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
        // 1. PRUEBAS DE OBTENER VALORACIONES POR PRODUCTO
        // ==========================================

        [Test]
        public async Task GetByProducto_SiProductoExiste_DebeDevolver200()
        {
            // PREPARAR
            var listaValoraciones = new List<ValoracionResponseDto>
            {
                new ValoracionResponseDto
                {
                    Id = 1, Estrellas = 5, Resena = "Excelente producto",
                    UsuarioId = TestUserId, UsuarioNombre = "Jorge",
                    ProductoId = 5, ProductoNombre = "Auriculares"
                }
            };

            _serviceFalso.Setup(s => s.GetValoracionesByProductoAsync(5))
                .ReturnsAsync(Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(listaValoraciones));

            // ACTUAR
            var resultado = await _controller.GetByProductoAsync(5);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetByProducto_SiProductoNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetValoracionesByProductoAsync(99))
                .ReturnsAsync(Result.Failure<IEnumerable<ValoracionResponseDto>, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.GetByProductoAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetByProducto_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetValoracionesByProductoAsync(5))
                .ReturnsAsync(Result.Failure<IEnumerable<ValoracionResponseDto>, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.GetByProductoAsync(5);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 2. PRUEBAS DE OBTENER VALORACIONES DEL USUARIO
        // ==========================================

        [Test]
        public async Task GetByUser_DebeDevolver200ConLista()
        {
            // PREPARAR
            var listaValoraciones = new List<ValoracionResponseDto>
            {
                new ValoracionResponseDto
                {
                    Id = 1, Estrellas = 4, Resena = "Buen producto",
                    UsuarioId = TestUserId, ProductoId = 5
                }
            };

            _serviceFalso.Setup(s => s.GetValoracionesByUserAsync(TestUserId))
                .ReturnsAsync(Result.Success<IEnumerable<ValoracionResponseDto>, PandaError>(listaValoraciones));

            // ACTUAR
            var resultado = await _controller.GetByUserAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetByUser_SiHayError_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetValoracionesByUserAsync(TestUserId))
                .ReturnsAsync(Result.Failure<IEnumerable<ValoracionResponseDto>, PandaError>(new BadRequestError("Error interno")));

            // ACTUAR
            var resultado = await _controller.GetByUserAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 3. PRUEBAS DE CREAR VALORACIÓN
        // ==========================================

        [Test]
        public async Task Create_ConDatosCorrectos_DebeDevolver201()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 5, Resena = "Excelente" };
            var valoracionResp = new ValoracionResponseDto
            {
                Id = 1, Estrellas = 5, Resena = "Excelente",
                UsuarioId = TestUserId, ProductoId = 5
            };

            _serviceFalso.Setup(s => s.CreateValoracionAsync(TestUserId, dto))
                .ReturnsAsync(Result.Success<ValoracionResponseDto, PandaError>(valoracionResp));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<CreatedResult>());
        }

        [Test]
        public async Task Create_SiProductoNoExiste_DebeDevolver404()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 99, Estrellas = 5, Resena = "Genial" };

            _serviceFalso.Setup(s => s.CreateValoracionAsync(TestUserId, dto))
                .ReturnsAsync(Result.Failure<ValoracionResponseDto, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Create_SiYaExisteValoracion_DebeDevolver409()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 5, Resena = "Otra vez" };

            _serviceFalso.Setup(s => s.CreateValoracionAsync(TestUserId, dto))
                .ReturnsAsync(Result.Failure<ValoracionResponseDto, PandaError>(new ConflictError("Ya has valorado este producto")));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Create_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 5, Resena = "Error" };

            _serviceFalso.Setup(s => s.CreateValoracionAsync(TestUserId, dto))
                .ReturnsAsync(Result.Failure<ValoracionResponseDto, PandaError>(new BadRequestError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.CreateAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 4. PRUEBAS DE ACTUALIZAR VALORACIÓN
        // ==========================================

        [Test]
        public async Task Update_ConDatosCorrectos_DebeDevolver200()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 4, Resena = "Actualizado" };
            var valoracionResp = new ValoracionResponseDto
            {
                Id = 1, Estrellas = 4, Resena = "Actualizado",
                UsuarioId = TestUserId, ProductoId = 5
            };

            _serviceFalso.Setup(s => s.UpdateValoracionAsync(1, TestUserId, dto))
                .ReturnsAsync(Result.Success<ValoracionResponseDto, PandaError>(valoracionResp));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Update_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 3, Resena = "No existe" };

            _serviceFalso.Setup(s => s.UpdateValoracionAsync(99, TestUserId, dto))
                .ReturnsAsync(Result.Failure<ValoracionResponseDto, PandaError>(new NotFoundError("Valoración no encontrada")));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(99, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Update_SiNoEsDelUsuario_DebeDevolver403()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 3, Resena = "Ajena" };

            _serviceFalso.Setup(s => s.UpdateValoracionAsync(1, TestUserId, dto))
                .ReturnsAsync(Result.Failure<ValoracionResponseDto, PandaError>(new OperacionNoPermitidaError("No puedes editar esta valoración")));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task Update_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            var dto = new CreateValoracionDto { ProductoId = 5, Estrellas = 3, Resena = "Error" };

            _serviceFalso.Setup(s => s.UpdateValoracionAsync(1, TestUserId, dto))
                .ReturnsAsync(Result.Failure<ValoracionResponseDto, PandaError>(new BadRequestError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.UpdateAsync(1, dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 5. PRUEBAS DE ELIMINAR VALORACIÓN
        // ==========================================

        [Test]
        public async Task Delete_SiExiste_DebeDevolver204()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteValoracionAsync(1, TestUserId))
                .ReturnsAsync(UnitResult.Success<PandaError>());

            // ACTUAR
            var resultado = await _controller.DeleteAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Delete_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteValoracionAsync(99, TestUserId))
                .ReturnsAsync(UnitResult.Failure(new NotFoundError("Valoración no encontrada") as PandaError));

            // ACTUAR
            var resultado = await _controller.DeleteAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Delete_SiNoEsDelUsuario_DebeDevolver403()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteValoracionAsync(1, TestUserId))
                .ReturnsAsync(UnitResult.Failure(new OperacionNoPermitidaError("No puedes eliminar esta valoración") as PandaError));

            // ACTUAR
            var resultado = await _controller.DeleteAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task Delete_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.DeleteValoracionAsync(1, TestUserId))
                .ReturnsAsync(UnitResult.Failure(new ConflictError("Error inesperado") as PandaError));

            // ACTUAR
            var resultado = await _controller.DeleteAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }
    }
}
