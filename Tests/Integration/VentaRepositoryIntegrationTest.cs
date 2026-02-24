using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Models;
using PandaBack.Repositories;

namespace Tests.Integration
{
    /// <summary>
    /// Tests de integración de VentaRepository con PostgreSQL real (TestContainers).
    /// </summary>
    [TestFixture]
    public class VentaRepositoryIntegrationTest : PostgresIntegrationTestBase
    {
        private VentaRepository _repository = null!;
        private string _testUserId = null!;
        private string _otherUserId = null!;
        private long _testProductoId;

        [SetUp]
        public async Task PrepararRepositorio()
        {
            var logger = new Mock<ILogger<VentaRepository>>();
            _repository = new VentaRepository(Context, logger.Object);

            // Crear usuarios de test
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "venta_test@test.com",
                NormalizedUserName = "VENTA_TEST@TEST.COM",
                Email = "venta_test@test.com",
                NormalizedEmail = "VENTA_TEST@TEST.COM",
                Nombre = "Venta",
                Apellidos = "Test",
                Role = Role.User
            };
            var otherUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "other_venta@test.com",
                NormalizedUserName = "OTHER_VENTA@TEST.COM",
                Email = "other_venta@test.com",
                NormalizedEmail = "OTHER_VENTA@TEST.COM",
                Nombre = "Other",
                Apellidos = "Venta",
                Role = Role.User
            };
            Context.Users.AddRange(user, otherUser);

            // Crear producto de test
            var producto = new Producto
            {
                Nombre = "Producto Venta", Precio = 100, Stock = 50,
                Category = Categoria.Gaming, IsDeleted = false
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            _testUserId = user.Id;
            _otherUserId = otherUser.Id;
            _testProductoId = producto.Id;
        }

        // ==========================================
        // GetAllAsync
        // ==========================================

        [Test]
        public async Task GetAllAsync_SinVentas_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetAllAsync();

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetAllAsync_ConVentas_DebeRetornarConUserYLineas()
        {
            var venta = new Venta
            {
                UserId = _testUserId,
                Total = 200,
                Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>
                {
                    new LineaVenta { ProductoId = _testProductoId, Cantidad = 2, PrecioUnitario = 100 }
                }
            };
            Context.Ventas.Add(venta);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetAllAsync();

            Assert.That(resultado.Count(), Is.EqualTo(1));
            Assert.That(resultado.First().User, Is.Not.Null);
            Assert.That(resultado.First().Lineas.Count, Is.EqualTo(1));
            Assert.That(resultado.First().Lineas.First().Producto, Is.Not.Null);
        }

        [Test]
        public async Task GetAllAsync_DebeOrdenarPorFechaDescendente()
        {
            Context.Ventas.Add(new Venta
            {
                UserId = _testUserId, Total = 100, Estado = EstadoPedido.Pendiente,
                FechaCompra = DateTime.UtcNow.AddDays(-2),
                Lineas = new List<LineaVenta>()
            });
            Context.Ventas.Add(new Venta
            {
                UserId = _otherUserId, Total = 200, Estado = EstadoPedido.Pendiente,
                FechaCompra = DateTime.UtcNow,
                Lineas = new List<LineaVenta>()
            });
            await Context.SaveChangesAsync();

            var resultado = (await _repository.GetAllAsync()).ToList();

            Assert.That(resultado.Count, Is.EqualTo(2));
            Assert.That(resultado[0].FechaCompra, Is.GreaterThan(resultado[1].FechaCompra));
        }

        // ==========================================
        // GetByUserIdAsync
        // ==========================================

        [Test]
        public async Task GetByUserIdAsync_SinVentas_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetByUserIdAsync_DebeRetornarSoloDelUsuario()
        {
            Context.Ventas.Add(new Venta
            {
                UserId = _testUserId, Total = 100, Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>()
            });
            Context.Ventas.Add(new Venta
            {
                UserId = _otherUserId, Total = 200, Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>()
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado.Count(), Is.EqualTo(1));
            Assert.That(resultado.First().UserId, Is.EqualTo(_testUserId));
        }

        // ==========================================
        // GetByIdAsync
        // ==========================================

        [Test]
        public async Task GetByIdAsync_SiExiste_DebeRetornarConIncludes()
        {
            var venta = new Venta
            {
                UserId = _testUserId, Total = 100, Estado = EstadoPedido.Procesando,
                Lineas = new List<LineaVenta>
                {
                    new LineaVenta { ProductoId = _testProductoId, Cantidad = 1, PrecioUnitario = 100 }
                }
            };
            Context.Ventas.Add(venta);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(venta.Id);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.User, Is.Not.Null);
            Assert.That(resultado.Lineas.Count, Is.EqualTo(1));
            Assert.That(resultado.Lineas.First().Producto, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_SiNoExiste_DebeRetornarNull()
        {
            var resultado = await _repository.GetByIdAsync(9999);

            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // AddAsync
        // ==========================================

        [Test]
        public async Task AddAsync_DebeGuardarVentaConLineas()
        {
            var venta = new Venta
            {
                UserId = _testUserId,
                Total = 300,
                Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>
                {
                    new LineaVenta { ProductoId = _testProductoId, Cantidad = 3, PrecioUnitario = 100 }
                }
            };

            await _repository.AddAsync(venta);

            Assert.That(venta.Id, Is.GreaterThan(0));

            var encontrado = await _repository.GetByIdAsync(venta.Id);
            Assert.That(encontrado, Is.Not.Null);
            Assert.That(encontrado!.Lineas.Count, Is.EqualTo(1));
            Assert.That(encontrado.Total, Is.EqualTo(300));
        }

        // ==========================================
        // UpdateAsync
        // ==========================================

        [Test]
        public async Task UpdateAsync_DebeActualizarEstado()
        {
            var venta = new Venta
            {
                UserId = _testUserId, Total = 100, Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>()
            };
            Context.Ventas.Add(venta);
            await Context.SaveChangesAsync();

            venta.Estado = EstadoPedido.Enviado;
            await _repository.UpdateAsync(venta);

            var encontrado = await Context.Ventas.FindAsync(venta.Id);
            Assert.That(encontrado!.Estado, Is.EqualTo(EstadoPedido.Enviado));
        }
    }
}
