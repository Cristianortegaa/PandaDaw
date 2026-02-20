using Moq;
using PandaBack.Services;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Errors;

namespace Tests.Services
{
    public class VentaServiceTest
    {
        private VentaService _service;
        private Mock<IVentaRepository> _repoVentasFalso;
        private Mock<ICarritoRepository> _repoCarritoFalso;
        private Mock<IProductoRepository> _repoProductosFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _repoVentasFalso = new Mock<IVentaRepository>();
            _repoCarritoFalso = new Mock<ICarritoRepository>();
            _repoProductosFalso = new Mock<IProductoRepository>();

            _service = new VentaService(_repoVentasFalso.Object, _repoCarritoFalso.Object, _repoProductosFalso.Object);
        }

        [Test]
        public async Task ObtenerTodas_DebeDevolverListaConExito()
        {
            // PREPARAR
            var listaFalsa = new List<Venta>
            {
                new Venta { Id = 1, UserId = 1, Lineas = new List<LineaVenta>() },
                new Venta { Id = 2, UserId = 2, Lineas = new List<LineaVenta>() }
            };
            _repoVentasFalso.Setup(r => r.GetAllAsync()).ReturnsAsync(listaFalsa);

            // ACTUAR
            var resultado = await _service.GetAllVentasAsync();

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task ObtenerPorUsuario_DebeDevolverListaConExito()
        {
            // PREPARAR
            long userId = 1;
            var listaFalsa = new List<Venta>
            {
                new Venta { Id = 1, UserId = userId, Lineas = new List<LineaVenta>() }
            };
            _repoVentasFalso.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(listaFalsa);

            // ACTUAR
            var resultado = await _service.GetVentasByUserAsync(userId);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task ObtenerPorId_SiNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoVentasFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venta)null);

            // ACTUAR
            var resultado = await _service.GetVentaByIdAsync(99);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task ObtenerPorId_SiExiste_DebeDevolverVentaConExito()
        {
            // PREPARAR
            var venta = new Venta { Id = 1, UserId = 1, Lineas = new List<LineaVenta>() };
            _repoVentasFalso.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venta);

            // ACTUAR
            var resultado = await _service.GetVentaByIdAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Id, Is.EqualTo(1));
        }

        // ==========================================
        // 2. PRUEBAS DE CREAR VENTA DESDE CARRITO
        // ==========================================

        [Test]
        public async Task CrearVenta_SiCarritoNoExiste_DebeDarErrorCarritoVacio()
        {
            // PREPARAR
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((Carrito)null);

            // ACTUAR
            var resultado = await _service.CreateVentaFromCarritoAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<CarritoVacioError>());
        }

        [Test]
        public async Task CrearVenta_SiCarritoVacio_DebeDarErrorCarritoVacio()
        {
            // PREPARAR
            var carritoVacio = new Carrito
            {
                Id = 10,
                UserId = 1,
                LineasCarrito = new List<LineaCarrito>()
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(carritoVacio);

            // ACTUAR
            var resultado = await _service.CreateVentaFromCarritoAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<CarritoVacioError>());
        }

        [Test]
        public async Task CrearVenta_SiProductoNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            var carrito = new Carrito
            {
                Id = 10,
                UserId = 1,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 99, Cantidad = 1 }
                }
            };
            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(carrito);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Producto)null);

            // ACTUAR
            var resultado = await _service.CreateVentaFromCarritoAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task CrearVenta_SiStockInsuficiente_DebeDarError()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Stock = 1, Precio = 50 };
            var carrito = new Carrito
            {
                Id = 10,
                UserId = 1,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 10, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(carrito);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.CreateVentaFromCarritoAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<StockInsuficienteError>());
        }

        [Test]
        public async Task CrearVenta_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Stock = 20, Precio = 50 };
            var carrito = new Carrito
            {
                Id = 10,
                UserId = 1,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 2, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(carrito);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            var resultado = await _service.CreateVentaFromCarritoAsync(1);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Total, Is.EqualTo(100)); // 2 * 50
            _repoVentasFalso.Verify(r => r.AddAsync(It.IsAny<Venta>()), Times.Once);
            _repoCarritoFalso.Verify(r => r.DeleteAsync(carrito.Id), Times.Once);
        }

        [Test]
        public async Task CrearVenta_DebeReducirStockDelProducto()
        {
            // PREPARAR
            var producto = new Producto { Id = 5, Nombre = "Auriculares", Stock = 20, Precio = 50 };
            var carrito = new Carrito
            {
                Id = 10,
                UserId = 1,
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 5, Cantidad = 3, Producto = producto }
                }
            };

            _repoCarritoFalso.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(carrito);
            _repoProductosFalso.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(producto);

            // ACTUAR
            await _service.CreateVentaFromCarritoAsync(1);

            // COMPROBAR: El stock debe haberse reducido
            Assert.That(producto.Stock, Is.EqualTo(17)); // 20 - 3
        }

        // ==========================================
        // 3. PRUEBAS DE ACTUALIZAR ESTADO
        // ==========================================

        [Test]
        public async Task ActualizarEstado_SiVentaNoExiste_DebeDarErrorNotFound()
        {
            // PREPARAR
            _repoVentasFalso.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Venta)null);

            // ACTUAR
            var resultado = await _service.UpdateEstadoVentaAsync(99, EstadoPedido.Enviado);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<NotFoundError>());
        }

        [Test]
        public async Task ActualizarEstado_ConTransicionValida_DebeTenerExito()
        {
            // PREPARAR
            var venta = new Venta
            {
                Id = 1,
                UserId = 1,
                Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>()
            };
            _repoVentasFalso.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venta);

            // ACTUAR
            var resultado = await _service.UpdateEstadoVentaAsync(1, EstadoPedido.Procesando);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Estado, Is.EqualTo("Procesando"));
        }

        [Test]
        public async Task ActualizarEstado_SiPedidoCancelado_DebeDarErrorOperacionNoPermitida()
        {
            // PREPARAR
            var venta = new Venta
            {
                Id = 1,
                UserId = 1,
                Estado = EstadoPedido.Cancelado,
                Lineas = new List<LineaVenta>()
            };
            _repoVentasFalso.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venta);

            // ACTUAR
            var resultado = await _service.UpdateEstadoVentaAsync(1, EstadoPedido.Enviado);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<OperacionNoPermitidaError>());
        }

        [Test]
        public async Task ActualizarEstado_SiPedidoEntregadoYCambiado_DebeDarErrorOperacionNoPermitida()
        {
            // PREPARAR
            var venta = new Venta
            {
                Id = 1,
                UserId = 1,
                Estado = EstadoPedido.Entregado,
                Lineas = new List<LineaVenta>()
            };
            _repoVentasFalso.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venta);

            // ACTUAR
            var resultado = await _service.UpdateEstadoVentaAsync(1, EstadoPedido.Pendiente);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Is.InstanceOf<OperacionNoPermitidaError>());
        }
    }
}

