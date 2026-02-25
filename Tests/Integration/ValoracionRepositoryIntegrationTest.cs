using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Models;
using PandaBack.Repositories;

namespace Tests.Integration
{
    /// <summary>
    /// Tests de integración de ValoracionRepository con PostgreSQL real (TestContainers).
    /// </summary>
    [TestFixture]
    public class ValoracionRepositoryIntegrationTest : PostgresIntegrationTestBase
    {
        private ValoracionRepository _repository = null!;
        private string _testUserId = null!;
        private string _otherUserId = null!;
        private long _testProductoId;
        private long _otherProductoId;

        [SetUp]
        public async Task PrepararRepositorio()
        {
            var logger = new Mock<ILogger<ValoracionRepository>>();
            _repository = new ValoracionRepository(Context, logger.Object);

            // Crear usuarios de test
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "val_test@test.com",
                NormalizedUserName = "VAL_TEST@TEST.COM",
                Email = "val_test@test.com",
                NormalizedEmail = "VAL_TEST@TEST.COM",
                Nombre = "Val",
                Apellidos = "Test",
                Role = Role.User
            };
            var otherUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "other@test.com",
                NormalizedUserName = "OTHER@TEST.COM",
                Email = "other@test.com",
                NormalizedEmail = "OTHER@TEST.COM",
                Nombre = "Other",
                Apellidos = "User",
                Role = Role.User
            };
            Context.Users.AddRange(user, otherUser);

            // Crear productos de test
            var producto1 = new Producto
            {
                Nombre = "Producto 1", Precio = 100, Stock = 10,
                Category = Categoria.Audio, IsDeleted = false
            };
            var producto2 = new Producto
            {
                Nombre = "Producto 2", Precio = 200, Stock = 5,
                Category = Categoria.Gaming, IsDeleted = false
            };
            Context.Productos.AddRange(producto1, producto2);
            await Context.SaveChangesAsync();

            _testUserId = user.Id;
            _otherUserId = otherUser.Id;
            _testProductoId = producto1.Id;
            _otherProductoId = producto2.Id;
        }

        // ==========================================
        // GetByProductoIdAsync
        // ==========================================

        [Test]
        public async Task GetByProductoIdAsync_SinValoraciones_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetByProductoIdAsync(_testProductoId);

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetByProductoIdAsync_ConValoraciones_DebeRetornarConUsuarioYProducto()
        {
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _testUserId,
                ProductoId = _testProductoId,
                Estrellas = 5,
                Resena = "Excelente"
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByProductoIdAsync(_testProductoId);

            Assert.That(resultado.Count(), Is.EqualTo(1));
            Assert.That(resultado.First().User, Is.Not.Null);
            Assert.That(resultado.First().Producto, Is.Not.Null);
            Assert.That(resultado.First().Estrellas, Is.EqualTo(5));
        }

        [Test]
        public async Task GetByProductoIdAsync_NoDebeRetornarDeOtroProducto()
        {
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _testUserId,
                ProductoId = _otherProductoId,
                Estrellas = 3,
                Resena = "Regular"
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByProductoIdAsync(_testProductoId);

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetByProductoIdAsync_DebeOrdenarPorFechaDescendente()
        {
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _testUserId,
                ProductoId = _testProductoId,
                Estrellas = 3,
                Resena = "Primera",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _otherUserId,
                ProductoId = _testProductoId,
                Estrellas = 5,
                Resena = "Segunda",
                CreatedAt = DateTime.UtcNow
            });
            await Context.SaveChangesAsync();

            var resultado = (await _repository.GetByProductoIdAsync(_testProductoId)).ToList();

            Assert.That(resultado.Count, Is.EqualTo(2));
            Assert.That(resultado[0].Resena, Is.EqualTo("Segunda")); // La más reciente primero
        }

        // ==========================================
        // GetByUserIdAsync
        // ==========================================

        [Test]
        public async Task GetByUserIdAsync_SinValoraciones_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetByUserIdAsync_ConValoraciones_DebeRetornarSoloDelUsuario()
        {
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _testUserId, ProductoId = _testProductoId,
                Estrellas = 4, Resena = "Bueno"
            });
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _otherUserId, ProductoId = _testProductoId,
                Estrellas = 2, Resena = "Malo"
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
            var valoracion = new Valoracion
            {
                UserId = _testUserId, ProductoId = _testProductoId,
                Estrellas = 5, Resena = "Top"
            };
            Context.Valoraciones.Add(valoracion);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(valoracion.Id);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.User, Is.Not.Null);
            Assert.That(resultado.Producto, Is.Not.Null);
        }

        [Test]
        public async Task GetByIdAsync_SiNoExiste_DebeRetornarNull()
        {
            var resultado = await _repository.GetByIdAsync(9999);

            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // GetByProductoAndUserAsync
        // ==========================================

        [Test]
        public async Task GetByProductoAndUserAsync_SiExiste_DebeRetornarValoracion()
        {
            Context.Valoraciones.Add(new Valoracion
            {
                UserId = _testUserId, ProductoId = _testProductoId,
                Estrellas = 4, Resena = "Bien"
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByProductoAndUserAsync(_testProductoId, _testUserId);

            Assert.That(resultado, Is.Not.Null);
        }

        [Test]
        public async Task GetByProductoAndUserAsync_SiNoExiste_DebeRetornarNull()
        {
            var resultado = await _repository.GetByProductoAndUserAsync(9999, _testUserId);

            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // AddAsync
        // ==========================================

        [Test]
        public async Task AddAsync_DebeGuardarValoracion()
        {
            var valoracion = new Valoracion
            {
                UserId = _testUserId, ProductoId = _testProductoId,
                Estrellas = 5, Resena = "Genial"
            };

            await _repository.AddAsync(valoracion);

            Assert.That(valoracion.Id, Is.GreaterThan(0));
        }

        // ==========================================
        // UpdateAsync
        // ==========================================

        [Test]
        public async Task UpdateAsync_DebeActualizarValoracion()
        {
            var valoracion = new Valoracion
            {
                UserId = _testUserId, ProductoId = _testProductoId,
                Estrellas = 3, Resena = "Regular"
            };
            Context.Valoraciones.Add(valoracion);
            await Context.SaveChangesAsync();

            valoracion.Estrellas = 5;
            valoracion.Resena = "Actualizado a excelente";
            await _repository.UpdateAsync(valoracion);

            var encontrado = await Context.Valoraciones.FindAsync(valoracion.Id);
            Assert.That(encontrado!.Estrellas, Is.EqualTo(5));
            Assert.That(encontrado.Resena, Is.EqualTo("Actualizado a excelente"));
        }

        // ==========================================
        // DeleteAsync
        // ==========================================

        [Test]
        public async Task DeleteAsync_SiExiste_DebeEliminarValoracion()
        {
            var valoracion = new Valoracion
            {
                UserId = _testUserId, ProductoId = _testProductoId,
                Estrellas = 1, Resena = "Horrible"
            };
            Context.Valoraciones.Add(valoracion);
            await Context.SaveChangesAsync();

            await _repository.DeleteAsync(valoracion.Id);

            var encontrado = await Context.Valoraciones.FindAsync(valoracion.Id);
            Assert.That(encontrado, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_SiNoExiste_NoDebeLanzarError()
        {
            Assert.DoesNotThrowAsync(async () => await _repository.DeleteAsync(9999));
        }
    }
}
