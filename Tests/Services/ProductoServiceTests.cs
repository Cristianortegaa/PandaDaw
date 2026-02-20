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

        // ==========================================
        // 1. PRUEBAS DE CREAR PRODUCTO
        // ==========================================

        [Test]
        public async Task CrearProducto_SiPrecioEsNegativo_DebeDarError()
        {
            // PREPARAR
            var productoMalo = new Producto { Nombre = "Hucha cerdito", Precio = -10, Stock = 5 };

            // ACTUAR
            var resultado = await _service.CreateProductoAsync(productoMalo);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task CrearProducto_SiStockEsNegativo_DebeDarError()
        {
            // PREPARAR
            var productoMalo = new Producto { Nombre = "Libreta de ahorro", Precio = 15, Stock = -2 };

            // ACTUAR
            var resultado = await _service.CreateProductoAsync(productoMalo);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task CrearProducto_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var productoBueno = new Producto { Nombre = "Agenda financiera", Precio = 20, Stock = 10 };

            // ACTUAR
            var resultado = await _service.CreateProductoAsync(productoBueno);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Agenda financiera"));
        }

        // ==========================================
        // 2. PRUEBAS DE OBTENER PRODUCTOS
        // ==========================================

        [Test]
        public async Task ObtenerTodos_DebeDevolverLaListaDelRepositorio()
        {
            // PREPARAR
            // Creamos una lista falsa de productos
            var listaFalsa = new List<Producto> 
            { 
                new Producto { Id = 1, Nombre = "Hucha cerdito" },
                new Producto { Id = 2, Nombre = "Agenda financiera" }
            };
            
            // Le "enseñamos" al repositorio falso que cuando le pidan todos, devuelva esta lista
            _repoFalso.Setup(repo => repo.GetAllAsync()).ReturnsAsync(listaFalsa);

            // ACTUAR
            var resultado = await _service.GetAllProductosAsync();

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(2)); // Verificamos que trae 2 elementos
        }

        [Test]
        public async Task ObtenerPorId_SiNoExisteEnBd_DebeDarErrorNotFound()
        {
            // PREPARAR
            long idQueNoExiste = 99;
            
            // Le decimos a la caché que no tiene nada (devuelve null)
            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((byte[])null);
                       
            // Le decimos al repo falso que tampoco encuentra el producto (devuelve null)
            _repoFalso.Setup(repo => repo.GetByIdAsync(idQueNoExiste)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.GetProductoByIdAsync(idQueNoExiste);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True); // Debe fallar porque no existe
        }

        // ==========================================
        // 3. PRUEBAS DE ACTUALIZAR
        // ==========================================

        [Test]
        public async Task ActualizarProducto_SiNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            long idQueNoExiste = 99;
            var datosNuevos = new Producto { Nombre = "Monedero digital", Precio = 10 };
            
            _repoFalso.Setup(repo => repo.GetByIdAsync(idQueNoExiste)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.UpdateProductoAsync(idQueNoExiste, datosNuevos);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task ActualizarProducto_ConDatosCorrectos_DebeActualizarConExito()
        {
            // PREPARAR
            long id = 1;
            var productoViejoEnBd = new Producto { Id = id, Nombre = "Hucha de lata", Precio = 5 };
            var datosNuevos = new Producto { Nombre = "Hucha de cerámica", Precio = 15 };
            
            // Simulamos que el repositorio sí encuentra el producto viejo
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(productoViejoEnBd);

            // ACTUAR
            var resultado = await _service.UpdateProductoAsync(id, datosNuevos);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Hucha de cerámica")); // El nombre tuvo que cambiar
            Assert.That(resultado.Value.Precio, Is.EqualTo(15)); // El precio tuvo que cambiar
        }

        // ==========================================
        // 4. PRUEBAS DE ELIMINAR
        // ==========================================

        [Test]
        public async Task EliminarProducto_SiExiste_DebeTenerExito()
        {
            // PREPARAR
            long id = 1;
            var productoEnBd = new Producto { Id = id, Nombre = "Libreta de ahorro" };
            
            // Simulamos que el producto sí existe para poder borrarlo
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(productoEnBd);

            // ACTUAR
            var resultado = await _service.DeleteProductoAsync(id);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
        }

        [Test]
        public async Task GetProductoByIdAsync_CuandoEstaEnCache_DebeRetornarDesdeCache()
        {
            // PREPARAR: Simulamos que la caché tiene un producto guardado
            long id = 1;
            var productoCacheado = new Producto { Id = id, Nombre = "Producto en Caché" };
            var json = JsonSerializer.Serialize(productoCacheado);
            var bytes = Encoding.UTF8.GetBytes(json);

            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(bytes);

            // ACTUAR
            var resultado = await _service.GetProductoByIdAsync(id);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Producto en Caché"));
            // Verificamos que NO fue al repositorio (porque lo encontró en caché)
            _repoFalso.Verify(repo => repo.GetByIdAsync(id), Times.Never);
        }

        [Test]
        public async Task GetProductosByCategoryAsync_DebeLlamarAlRepositorio()
        {
            // PREPARAR
            var categoria = Categoria.Audio; 
            var listaFalsa = new List<Producto> { new Producto { Nombre = "Producto Categoría" } };
            _repoFalso.Setup(repo => repo.GetByCategoryAsync(categoria)).ReturnsAsync(listaFalsa);

            // ACTUAR
            var resultado = await _service.GetProductosByCategoryAsync(categoria);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateProductoAsync_SiPrecioEsNegativo_DebeDarError()
        {
            // PREPARAR
            long id = 1;
            var productoExistente = new Producto { Id = id, Nombre = "Existente" };
            var datosNuevosMalos = new Producto { Precio = -10 }; // Precio negativo
            
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(productoExistente);

            // ACTUAR
            var resultado = await _service.UpdateProductoAsync(id, datosNuevosMalos);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
        }

        [Test]
        public async Task GetProductoByIdAsync_CuandoEstaEnCache_PeroEsInvalido_DebeIrARepositorio()
        {
            // PREPARAR: Ponemos algo en la caché que no sea un Producto válido (o un null serializado)
            long id = 1;
            var bytesMalo = Encoding.UTF8.GetBytes("null"); 

            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(bytesMalo);
                       
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(new Producto { Id = id });

            // ACTUAR
            await _service.GetProductoByIdAsync(id);

            // COMPROBAR: Debe haber saltado el if de caché e ir al repo
            _repoFalso.Verify(repo => repo.GetByIdAsync(id), Times.Once);
        }

        [Test]
        public async Task GetProductoByIdAsync_CuandoNoEstaEnCache_DebeGuardarloAlFinal()
        {
            // PREPARAR
            long id = 1;
            var producto = new Producto { Id = id, Nombre = "Test" };
            _cacheFalsa.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((byte[])null);
            _repoFalso.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(producto);

            // ACTUAR
            await _service.GetProductoByIdAsync(id);

            // COMPROBAR: Verificamos que se llamó a SetAsync para guardar en la caché
            _cacheFalsa.Verify(c => c.SetAsync(
                It.Is<string>(k => k.Contains(id.ToString())), 
                It.IsAny<byte[]>(), 
                It.IsAny<DistributedCacheEntryOptions>(), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteProductoAsync_SiNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            long idInexistente = 999;
            _repoFalso.Setup(repo => repo.GetByIdAsync(idInexistente)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.DeleteProductoAsync(idInexistente);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
            _repoFalso.Verify(repo => repo.DeleteAsync(It.IsAny<long>()), Times.Never);
        }
    }
}