using Moq;
using Microsoft.Extensions.Caching.Distributed;
using PandaBack.Services;
using PandaBack.Models;
using PandaBack.Repositories;
using System.Text;
using System.Text.Json;
using PandaBack.Errors;

namespace Tests.Services
{
    public class ProductoServiceTest
    {
        private ProductoService _service;
        private Mock<IProductoRepository> _repoFalso;
        private Mock<IDistributedCache> _cacheFalsa;

        [SetUp]
        public void PrepararTodo()
        {
            _repoFalso = new Mock<IProductoRepository>();
            _cacheFalsa = new Mock<IDistributedCache>();
            _service = new ProductoService(_repoFalso.Object, _cacheFalsa.Object);
        }
        
        [Test]
        public async Task CrearProducto_SiPrecioEsNegativo_DebeDarError()
        {
            var productoMalo = new Producto { Nombre = "Hucha cerdito", Precio = -10, Stock = 5 };

            var resultado = await _service.CreateProductoAsync(productoMalo);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task CrearProducto_SiStockEsNegativo_DebeDarError()
        {
            var productoMalo = new Producto { Nombre = "Libreta de ahorro", Precio = 15, Stock = -2 };

            var resultado = await _service.CreateProductoAsync(productoMalo);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task CrearProducto_ConDatosCorrectos_DebeTenerExito()
        {
            var productoBueno = new Producto { Nombre = "Agenda financiera", Precio = 20, Stock = 10 };

            var resultado = await _service.CreateProductoAsync(productoBueno);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Agenda financiera"));
        }
        

        [Test]
        public async Task ObtenerTodos_DebeDevolverLaListaDelRepositorio()
        {
            var listaFalsa = new List<Producto> 
            { 
                new Producto { Id = 1, Nombre = "Hucha cerdito" },
                new Producto { Id = 2, Nombre = "Agenda financiera" }
            };
            
            _repoFalso.Setup(repo => repo.GetAllAsync()).ReturnsAsync(listaFalsa);

            var resultado = await _service.GetAllProductosAsync();

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(2)); 
        }

        [Test]
        public async Task ObtenerPorId_SiNoExisteEnBd_DebeDarErrorNotFound()
        {
            long idQueNoExiste = 99;
            
            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((byte[])null);
                       
            _repoFalso.Setup(repo => repo.GetByIdAsync(idQueNoExiste)).ReturnsAsync((Producto)null);

            var resultado = await _service.GetProductoByIdAsync(idQueNoExiste);

            Assert.That(resultado.IsFailure, Is.True); 
        }


        [Test]
        public async Task ActualizarProducto_SiNoExiste_DebeDarErrorNotFound()
        {
            long idQueNoExiste = 99;
            var datosNuevos = new Producto { Nombre = "Monedero digital", Precio = 10 };
            
            _repoFalso.Setup(repo => repo.GetByIdAsync(idQueNoExiste)).ReturnsAsync((Producto)null);

            var resultado = await _service.UpdateProductoAsync(idQueNoExiste, datosNuevos);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task ActualizarProducto_ConDatosCorrectos_DebeActualizarConExito()
        {
            long id = 1;
            var productoViejoEnBd = new Producto { Id = id, Nombre = "Hucha de lata", Precio = 5 };
            var datosNuevos = new Producto { Nombre = "Hucha de cerámica", Precio = 15 };
            
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(productoViejoEnBd);

            var resultado = await _service.UpdateProductoAsync(id, datosNuevos);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Hucha de cerámica"));
            Assert.That(resultado.Value.Precio, Is.EqualTo(15)); 
        }


        [Test]
        public async Task EliminarProducto_SiExiste_DebeTenerExito()
        {
            long id = 1;
            var productoEnBd = new Producto { Id = id, Nombre = "Libreta de ahorro" };
            
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(productoEnBd);

            var resultado = await _service.DeleteProductoAsync(id);

            Assert.That(resultado.IsSuccess, Is.True);
        }

        [Test]
        public async Task GetProductoByIdAsync_CuandoEstaEnCache_DebeRetornarDesdeCache()
        {
            long id = 1;
            var productoCacheado = new Producto { Id = id, Nombre = "Producto en Caché" };
            var json = JsonSerializer.Serialize(productoCacheado);
            var bytes = Encoding.UTF8.GetBytes(json);

            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(bytes);

            var resultado = await _service.GetProductoByIdAsync(id);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Producto en Caché"));
            _repoFalso.Verify(repo => repo.GetByIdAsync(id), Times.Never);
        }

        [Test]
        public async Task GetProductosByCategoryAsync_DebeLlamarAlRepositorio()
        {
            var categoria = Categoria.Audio; 
            var listaFalsa = new List<Producto> { new Producto { Nombre = "Producto Categoría" } };
            _repoFalso.Setup(repo => repo.GetByCategoryAsync(categoria)).ReturnsAsync(listaFalsa);

            var resultado = await _service.GetProductosByCategoryAsync(categoria);

            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateProductoAsync_SiPrecioEsNegativo_DebeDarError()
        {
            long id = 1;
            var productoExistente = new Producto { Id = id, Nombre = "Existente" };
            var datosNuevosMalos = new Producto { Precio = -10 }; 
            
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(productoExistente);

            var resultado = await _service.UpdateProductoAsync(id, datosNuevosMalos);

            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task GetProductoByIdAsync_CuandoEstaEnCache_PeroEsInvalido_DebeIrARepositorio()
        {
            long id = 1;
            var bytesMalo = Encoding.UTF8.GetBytes("null"); 

            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(bytesMalo);
                       
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(new Producto { Id = id });

            await _service.GetProductoByIdAsync(id);

            _repoFalso.Verify(repo => repo.GetByIdAsync(id), Times.Once);
        }

        [Test]
        public async Task GetProductoByIdAsync_CuandoNoEstaEnCache_DebeGuardarloAlFinal()
        {
            long id = 1;
            var producto = new Producto { Id = id, Nombre = "Test" };
            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((byte[])null);
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(producto);

            await _service.GetProductoByIdAsync(id);

            _cacheFalsa.Verify(c => c.SetAsync(
                It.Is<string>(k => k.Contains(id.ToString())), 
                It.IsAny<byte[]>(), 
                It.IsAny<DistributedCacheEntryOptions>(), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteProductoAsync_SiNoExiste_DebeDarErrorNotFound()
        {
            long idInexistente = 999;
            _repoFalso.Setup(repo => repo.GetByIdAsync(idInexistente)).ReturnsAsync((Producto)null);

            var resultado = await _service.DeleteProductoAsync(idInexistente);

            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
            _repoFalso.Verify(repo => repo.DeleteAsync(It.IsAny<long>()), Times.Never);
        }
    }
}