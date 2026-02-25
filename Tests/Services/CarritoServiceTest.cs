﻿using Moq;
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
        private const string TestUserId = "test-user-id";

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
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>()
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.GetCarritoByUserIdAsync(TestUserId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Id, Is.EqualTo(10));
        }

        [Test]
        public async Task ObtenerCarrito_SiNoExiste_DebeCrearUnoVacio()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito)null!);

            // ACTUAR
            var resultado = await _service.GetCarritoByUserIdAsync(TestUserId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            _repoCarritoFalso.Verify(r => r.AddAsync(It.Is<Carrito>(c => c.UserId == TestUserId)), Times.Once);
        }

        // ==========================================
        // 2. PRUEBAS DE AÑADIR LÍNEA AL CARRITO
        // ==========================================

        [Test]
        public async Task AddLinea_SiCantidadEsCero_DebeDarError()
        {
            // PREPARAR & ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 5, 0);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        }

        [Test]
        public async Task AddLinea_SiCantidadEsNegativa_DebeDarError()
        {
            // PREPARAR & ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 5, -3);

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
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 99, 2);

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
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 5, 10);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<StockInsuficienteError>());
        }

        [Test]
        public async Task AddLinea_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 20 };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 5, 2);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
        }

        [Test]
        public async Task AddLinea_SiProductoYaEstaEnCarrito_DebeIncrementarCantidad()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 20 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 2, Producto = producto }
                }
            };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 5, 3);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(carritoExistente.LineasCarrito.First().Cantidad, Is.EqualTo(5));
        }

        [Test]
        public async Task AddLinea_SiProductoYaEstaYStockInsuficiente_DebeDarError()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 5 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 3, Producto = producto }
                }
            };

            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.AddLineaCarritoAsync(TestUserId, 5, 5);

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
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 5, 0);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        }

        [Test]
        public async Task UpdateCantidad_SiCarritoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 5, 2);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task UpdateCantidad_SiProductoNoEstaEnCarrito_DebeDarErrorNotFound()
        {
            // PREPARAR
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 99, 2);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task UpdateCantidad_SiCantidadNegativa_DebeDarError()
        {
            // PREPARAR & ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 5, -3);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<BadRequestError>());
        }

        [Test]
        public async Task UpdateCantidad_SiProductoYaNoExisteEnBd_DebeDarErrorNotFound()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 10 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 1, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 5, 2);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task UpdateCantidad_SiStockInsuficiente_DebeDarError()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 3 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 1, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 5, 10);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<StockInsuficienteError>());
        }

        [Test]
        public async Task UpdateCantidad_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Precio = 50, Stock = 20 };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 1, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.UpdateLineaCantidadAsync(TestUserId, 5, 5);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(carritoExistente.LineasCarrito.First().Cantidad, Is.EqualTo(5));
            _repoCarritoFalso.Verify(r => r.UpdateAsync(carritoExistente), Times.Once);
        }

        // ==========================================
        // 4. PRUEBAS DE ELIMINAR LÍNEA
        // ==========================================

        [Test]
        public async Task RemoveLinea_SiCarritoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.RemoveLineaCarritoAsync(TestUserId, 5);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task RemoveLinea_SiProductoNoEstaEnCarrito_DebeDarErrorNotFound()
        {
            // PREPARAR
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.RemoveLineaCarritoAsync(TestUserId, 99);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task RemoveLinea_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares" };
            var linea = new LineaCarrito { ProductoId = 5, Cantidad = 2, Producto = producto };
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito> { linea }
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.RemoveLineaCarritoAsync(TestUserId, 5);

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
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.VaciarCarritoAsync(TestUserId);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task VaciarCarrito_SiExiste_DebeTenerExito()
        {
            // PREPARAR
            var carritoExistente = new Carrito
            {
                Id = 10,
                UserId = TestUserId,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(TestUserId)).ReturnsAsync(carritoExistente);

            // ACTUAR
            var resultado = await _service.VaciarCarritoAsync(TestUserId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            _repoCarritoFalso.Verify(r => r.DeleteAsync(carritoExistente.Id), Times.Once);
        }
    }
}
