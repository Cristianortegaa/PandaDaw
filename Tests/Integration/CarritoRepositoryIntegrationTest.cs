using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Models;
using PandaBack.Repositories;

namespace Tests.Integration
{
    /// <summary>
    /// Tests de integración de CarritoRepository con PostgreSQL real (TestContainers).
    /// </summary>
    [TestFixture]
    public class CarritoRepositoryIntegrationTest : PostgresIntegrationTestBase
    {
        private CarritoRepository _repository = null!;
        private string _testUserId = null!;

        [SetUp]
        public async Task PrepararRepositorio()
        {
            var logger = new Mock<ILogger<CarritoRepository>>();
            _repository = new CarritoRepository(Context, logger.Object);

            // Crear un usuario de test para las FK
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "testuser@test.com",
                NormalizedUserName = "TESTUSER@TEST.COM",
                Email = "testuser@test.com",
                NormalizedEmail = "TESTUSER@TEST.COM",
                Nombre = "Test",
                Apellidos = "User",
                Role = Role.User
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            _testUserId = user.Id;
        }

        // ==========================================
        // GetByUserIdAsync
        // ==========================================

        [Test]
        public async Task GetByUserIdAsync_SinCarrito_DebeRetornarNull()
        {
            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado, Is.Null);
        }

        [Test]
        public async Task GetByUserIdAsync_ConCarrito_DebeRetornarConLineas()
        {
            // PREPARAR
            var producto = new Producto
            {
                Nombre = "Test", Precio = 50, Stock = 10,
                Category = Categoria.Audio, IsDeleted = false
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            var carrito = new Carrito { UserId = _testUserId };
            carrito.LineasCarrito.Add(new LineaCarrito { ProductoId = producto.Id, Cantidad = 2 });
            Context.Carritos.Add(carrito);
            await Context.SaveChangesAsync();

            // ACTUAR
            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            // COMPROBAR
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.UserId, Is.EqualTo(_testUserId));
            Assert.That(resultado.LineasCarrito.Count, Is.EqualTo(1));
            Assert.That(resultado.LineasCarrito.First().Producto, Is.Not.Null);
            Assert.That(resultado.LineasCarrito.First().Producto!.Nombre, Is.EqualTo("Test"));
        }

        // ==========================================
        // GetByIdAsync
        // ==========================================

        [Test]
        public async Task GetByIdAsync_SiExiste_DebeRetornarCarrito()
        {
            var carrito = new Carrito { UserId = _testUserId };
            Context.Carritos.Add(carrito);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(carrito.Id);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.Id, Is.EqualTo(carrito.Id));
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
        public async Task AddAsync_DebeCrearCarrito()
        {
            var carrito = new Carrito { UserId = _testUserId };

            await _repository.AddAsync(carrito);

            Assert.That(carrito.Id, Is.GreaterThan(0));
            var encontrado = await Context.Carritos.FindAsync(carrito.Id);
            Assert.That(encontrado, Is.Not.Null);
        }

        // ==========================================
        // UpdateAsync
        // ==========================================

        [Test]
        public async Task UpdateAsync_DebeActualizarCarrito()
        {
            var producto = new Producto
            {
                Nombre = "Test", Precio = 50, Stock = 10,
                Category = Categoria.Audio, IsDeleted = false
            };
            Context.Productos.Add(producto);

            var carrito = new Carrito { UserId = _testUserId };
            Context.Carritos.Add(carrito);
            await Context.SaveChangesAsync();

            // Añadir una línea
            carrito.LineasCarrito.Add(new LineaCarrito { ProductoId = producto.Id, Cantidad = 3 });
            await _repository.UpdateAsync(carrito);

            // Verificar
            var encontrado = await _repository.GetByIdAsync(carrito.Id);
            Assert.That(encontrado!.LineasCarrito.Count, Is.EqualTo(1));
            Assert.That(encontrado.LineasCarrito.First().Cantidad, Is.EqualTo(3));
        }

        // ==========================================
        // DeleteAsync
        // ==========================================

        [Test]
        public async Task DeleteAsync_SiExiste_DebeEliminarCarrito()
        {
            var carrito = new Carrito { UserId = _testUserId };
            Context.Carritos.Add(carrito);
            await Context.SaveChangesAsync();

            await _repository.DeleteAsync(carrito.Id);

            var encontrado = await Context.Carritos.FindAsync(carrito.Id);
            Assert.That(encontrado, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_SiNoExiste_NoDebeLanzarError()
        {
            Assert.DoesNotThrowAsync(async () => await _repository.DeleteAsync(9999));
        }
    }
}
