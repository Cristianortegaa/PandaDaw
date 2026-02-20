using Moq;
using PandaBack.Services;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Errors;

namespace Tests.Services
{
    public class CarritoServiceTest
    {
        private CarritoService _service;
        private Mock<ICarritoRepository> _repoCarritoFalso;
        private Mock<IProductoRepository> _repoProductosFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _repoCarritoFalso = new Mock<ICarritoRepository>();
            _repoProductosFalso = new Mock<IProductoRepository>();

            _service = new CarritoService(_repoCarritoFalso.Object, _repoProductosFalso.Object);
        }

        [Test]
        public async Task ObtenerCarrito_SiExiste_DebeDevolverCarritoConExito()
        {
            // PREPARAR
            long userId = 1;
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>()
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.GetCarritoByUserIdAsync(userId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Id, Is.EqualTo(10));
        }

        [Test]
        public async Task ObtenerCarrito_SiNoExiste_DebeCrearUnoVacio()
        {
            // PREPARAR
            long userId = 1;
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.GetCarritoByUserIdAsync(userId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            _repoCarritoFalso.Verify(r => r.AddAsync(It.IsAny<Carrito>()), Times.Once);
        }

        // ==========================================
        // 2. PRUEBAS DE AÑADIR LÍNEA AL CARRITO
        // ==========================================

        [Test]
        public async Task AddLinea_SiCantidadEsCero_DebeDarError()
        {
            // PREPARAR & ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(1, 5, 0);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        }

        [Test]
        public async Task AddLinea_SiCantidadEsNegativa_DebeDarError()
        {
            // PREPARAR & ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(1, 5, -3);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        }

        [Test]
        public async Task AddLinea_SiProductoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoProductosFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(1, 99, 2);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task AddLinea_SiStockInsuficiente_DebeDarError()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Stock = 1 };
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(1, 5, 10);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<StockInsuficienteError>());
        }

        [Test]
        public async Task AddLinea_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 20 };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(userId, 5, 2);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
        }

        [Test]
        public async Task AddLinea_SiProductoYaEstaEnCarrito_DebeIncrementarCantidad()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 20 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 2, Producto = producto }
                }
            };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(userId, 5, 3);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(carritoExistente.LineasCarrito.First().Cantidad, Is.EqualTo(5));
        }

        [Test]
        public async Task AddLinea_SiProductoYaEstaYStockInsuficiente_DebeDarError()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 5 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 3, Producto = producto }
                }
            };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(userId, 5, 5);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<StockInsuficienteError>());
        }

        // ==========================================
        // 3. PRUEBAS DE ACTUALIZAR CANTIDAD
        // ==========================================

        [Test]
        public async Task UpdateCantidad_SiCantidadEsCero_DebeDarError()
        {
            // PREPARAR & ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(1, 5, 0);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        }

        [Test]
        public async Task UpdateCantidad_SiCarritoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(1, 5, 2);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task UpdateCantidad_SiProductoNoEstaEnCarrito_DebeDarErrorNotFound()
        {
            // PREPARAR
            long userId = 1;
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(userId, 99, 2);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task UpdateCantidad_SiStockInsuficiente_DebeDarError()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 3 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 1, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(userId, 5, 10);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<StockInsuficienteError>());
        }

        [Test]
        public async Task UpdateCantidad_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 20 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 1, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(userId, 5, 5);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(carritoExistente.LineasCarrito.First().Cantidad, Is.EqualTo(5));
        }

        // ==========================================
        // 4. PRUEBAS DE ELIMINAR LÍNEA
        // ==========================================

        [Test]
        public async Task RemoveLinea_SiCarritoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.RemoveLineaCarritoAsync(1, 5);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task RemoveLinea_SiProductoNoEstaEnCarrito_DebeDarErrorNotFound()
        {
            // PREPARAR
            long userId = 1;
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.RemoveLineaCarritoAsync(userId, 99);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task RemoveLinea_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            long userId = 1;
            var producto = new Producto { Id = 5, Nombre = "Auriculares" };
            var linea = new LineaCarrito { ProductoId = 5, Cantidad = 2, Producto = producto };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito> { linea }
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.RemoveLineaCarritoAsync(userId, 5);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
        }

        // ==========================================
        // 5. PRUEBAS DE VACIAR CARRITO
        // ==========================================

        [Test]
        public async Task VaciarCarrito_SiCarritoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.VaciarCarritoAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task VaciarCarrito_SiExiste_DebeTenerExito()
        {
            // PREPARAR
            long userId = 1;
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = userId,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.VaciarCarritoAsync(userId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            _repoCarritoFalso.Verify(r => r.DeleteAsync(carritoExistente.Id), Times.Once);
        }
    }
}

