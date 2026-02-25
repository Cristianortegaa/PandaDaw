using System.Security.Claims;
using CSharpFunctionalExtensions;
using Moq;
using PandaBack.Dtos.Favoritos;
using PandaBack.Errors;
using PandaBack.RestController;
using PandaBack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class FavoritosControllerTest
    {
        private FavoritosController _controller;
        private Mock<IFavoritoService> _serviceFalso;
        private const string TestUserId = "test-user-id";

        [SetUp]
        public void PrepararTodo()
        {
            _serviceFalso = new Mock<IFavoritoService>();
            _controller = new FavoritosController(_serviceFalso.Object);

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
        // 1. PRUEBAS DE OBTENER FAVORITOS
        // ==========================================

        [Test]
        public async Task GetFavoritos_SiHayFavoritos_DebeDevolver200()
        {
            // PREPARAR
            var listaFavoritos = new List<FavoritoResponseDto>
            {
                new FavoritoResponseDto { Id = 1, ProductoId = 5, ProductoNombre = "Auriculares", ProductoPrecio = 50 }
            };

            _serviceFalso.Setup(s => s.GetUserFavoritosAsync(TestUserId))
                .ReturnsAsync(Result.Success<IEnumerable<FavoritoResponseDto>, PandaError>(listaFavoritos));

            // ACTUAR
            var resultado = await _controller.GetFavoritosAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetFavoritos_SiHayError_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.GetUserFavoritosAsync(TestUserId))
                .ReturnsAsync(Result.Failure<IEnumerable<FavoritoResponseDto>, PandaError>(new BadRequestError("Error interno")));

            // ACTUAR
            var resultado = await _controller.GetFavoritosAsync();

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 2. PRUEBAS DE AÑADIR FAVORITO
        // ==========================================

        [Test]
        public async Task AddFavorito_ConDatosCorrectos_DebeDevolver201()
        {
            // PREPARAR
            var dto = new CreateFavoritoDto { ProductoId = 5 };
            var favoritoResp = new FavoritoResponseDto
            {
                Id = 1,
                ProductoId = 5,
                ProductoNombre = "Auriculares",
                ProductoPrecio = 50,
                AgregadoEl = DateTime.UtcNow
            };

            _serviceFalso.Setup(s => s.AddToFavoritosAsync(TestUserId, dto))
                .ReturnsAsync(Result.Success<FavoritoResponseDto, PandaError>(favoritoResp));

            // ACTUAR
            var resultado = await _controller.AddFavoritoAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<CreatedResult>());
        }

        [Test]
        public async Task AddFavorito_SiProductoNoExiste_DebeDevolver404()
        {
            // PREPARAR
            var dto = new CreateFavoritoDto { ProductoId = 99 };

            _serviceFalso.Setup(s => s.AddToFavoritosAsync(TestUserId, dto))
                .ReturnsAsync(Result.Failure<FavoritoResponseDto, PandaError>(new NotFoundError("Producto no encontrado")));

            // ACTUAR
            var resultado = await _controller.AddFavoritoAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AddFavorito_SiYaExiste_DebeDevolver400()
        {
            // PREPARAR
            var dto = new CreateFavoritoDto { ProductoId = 5 };

            _serviceFalso.Setup(s => s.AddToFavoritosAsync(TestUserId, dto))
                .ReturnsAsync(Result.Failure<FavoritoResponseDto, PandaError>(new BadRequestError("Ya está en favoritos")));

            // ACTUAR
            var resultado = await _controller.AddFavoritoAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddFavorito_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            var dto = new CreateFavoritoDto { ProductoId = 5 };

            _serviceFalso.Setup(s => s.AddToFavoritosAsync(TestUserId, dto))
                .ReturnsAsync(Result.Failure<FavoritoResponseDto, PandaError>(new ConflictError("Error inesperado")));

            // ACTUAR
            var resultado = await _controller.AddFavoritoAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        // ==========================================
        // 3. PRUEBAS DE ELIMINAR FAVORITO
        // ==========================================

        [Test]
        public async Task RemoveFavorito_SiExiste_DebeDevolver204()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.RemoveFromFavoritosAsync(1, TestUserId))
                .ReturnsAsync(UnitResult.Success<PandaError>());

            // ACTUAR
            var resultado = await _controller.RemoveFavoritoAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task RemoveFavorito_SiNoExiste_DebeDevolver404()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.RemoveFromFavoritosAsync(99, TestUserId))
                .ReturnsAsync(UnitResult.Failure(new NotFoundError("Favorito no encontrado") as PandaError));

            // ACTUAR
            var resultado = await _controller.RemoveFavoritoAsync(99);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task RemoveFavorito_SiHayErrorInterno_DebeDevolver500()
        {
            // PREPARAR
            _serviceFalso.Setup(s => s.RemoveFromFavoritosAsync(1, TestUserId))
                .ReturnsAsync(UnitResult.Failure(new ConflictError("Error inesperado") as PandaError));

            // ACTUAR
            var resultado = await _controller.RemoveFavoritoAsync(1);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<ObjectResult>());
            var objectResult = resultado as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }
    }
}
