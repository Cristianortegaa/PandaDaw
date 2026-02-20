using Moq;
using PandaBack.Services;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Dtos.Favoritos;
using PandaBack.Repository;

namespace Tests.Services
{
    public class FavoritoServicePruebasSencillas
    {
        private FavoritoService _service;
        private Mock<IFavoritoRepository> _repoFavoritosFalso;
        private Mock<IProductoRepository> _repoProductosFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _repoFavoritosFalso = new Mock<IFavoritoRepository>();
            _repoProductosFalso = new Mock<IProductoRepository>();
            
            _service = new FavoritoService(_repoFavoritosFalso.Object, _repoProductosFalso.Object);
        }

        [Test]
        public async Task ObtenerFavoritos_DebeDevolverListaConExito()
        {
            long userId = 1;
            var productoSimulado = new Producto { Nombre = "Test" }; 
            var listaFalsa = new List<Favorito> { new Favorito { ProductoId = 10, Producto = productoSimulado } };
    
            _repoFavoritosFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(listaFalsa);

            var resultado = await _service.GetUserFavoritosAsync(userId);

            Assert.That(resultado.IsSuccess, Is.True);
    
            Assert.That(resultado.Value.Count(), Is.EqualTo(1)); 
        }

        [Test]
        public async Task AgregarFavorito_SiProductoNoExiste_DebeDarError()
        {
            var dto = new CreateFavoritoDto { ProductoId = 99 }; 
            
            _repoProductosFalso.Setup(r => r.GetByIdAsync(dto.ProductoId)).ReturnsAsync((Producto)null);

            var resultado = await _service.AddToFavoritosAsync(1, dto);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task AgregarFavorito_SiYaEstaEnFavoritos_DebeDarError()
        {
            var dto = new CreateFavoritoDto { ProductoId = 5 };
            var productoReal = new Producto { Id = 5 };
            var favoritoRepetido = new Favorito { ProductoId = 5 };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(dto.ProductoId)).ReturnsAsync(productoReal);
            _repoFavoritosFalso.Setup(r => r.GetByProductAndUserAsync(dto.ProductoId, 1)).ReturnsAsync(favoritoRepetido);

            var resultado = await _service.AddToFavoritosAsync(1, dto);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task AgregarFavorito_ConDatosCorrectos_DebeTenerExito()
        {
            var dto = new CreateFavoritoDto { ProductoId = 5 };
            var productoReal = new Producto { Id = 5 };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(dto.ProductoId)).ReturnsAsync(productoReal);
            _repoFavoritosFalso.Setup(r => r.GetByProductAndUserAsync(dto.ProductoId, 1)).ReturnsAsync((Favorito)null);

            var resultado = await _service.AddToFavoritosAsync(1, dto);

            Assert.That(resultado.IsSuccess, Is.True);
        }

        [Test]
        public async Task BorrarFavorito_SiNoExiste_DebeDarError()
        {
            long favoritoId = 99; 
            _repoFavoritosFalso.Setup(r => r.GetByIdAsync(favoritoId)).ReturnsAsync((Favorito)null);

            var resultado = await _service.RemoveFromFavoritosAsync(favoritoId, 1);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task BorrarFavorito_SiExiste_DebeTenerExito()
        {
            long favoritoId = 10;
            var favoritoReal = new Favorito { Id = favoritoId };
            
            _repoFavoritosFalso.Setup(r => r.GetByIdAsync(favoritoId)).ReturnsAsync(favoritoReal);

            var resultado = await _service.RemoveFromFavoritosAsync(favoritoId, 1);

            Assert.That(resultado.IsSuccess, Is.True);
        }
    }
}