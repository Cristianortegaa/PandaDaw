using PandaBack.Models;
using PandaBack.Repository;

namespace Tests.Integration
{
    /// <summary>
    /// Tests de integración de FavoritoRepository con PostgreSQL real (TestContainers).
    /// </summary>
    [TestFixture]
    public class FavoritoRepositoryIntegrationTest : PostgresIntegrationTestBase
    {
        private FavoritoRepository _repository = null!;
        private string _testUserId = null!;
        private long _testProductoId;

        [SetUp]
        public async Task PrepararRepositorio()
        {
            _repository = new FavoritoRepository(Context);

            // Crear usuario de test
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "fav_test@test.com",
                NormalizedUserName = "FAV_TEST@TEST.COM",
                Email = "fav_test@test.com",
                NormalizedEmail = "FAV_TEST@TEST.COM",
                Nombre = "Fav",
                Apellidos = "Test",
                Role = Role.User
            };
            Context.Users.Add(user);

            // Crear producto de test
            var producto = new Producto
            {
                Nombre = "Producto Fav", Precio = 100, Stock = 10,
                Category = Categoria.Audio, IsDeleted = false
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            _testUserId = user.Id;
            _testProductoId = producto.Id;
        }

        // ==========================================
        // GetByUserIdAsync
        // ==========================================

        [Test]
        public async Task GetByUserIdAsync_SinFavoritos_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetByUserIdAsync_ConFavoritos_DebeRetornarConProducto()
        {
            Context.Favoritos.Add(new Favorito
            {
                UserId = _testUserId,
                ProductoId = _testProductoId
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado.Count(), Is.EqualTo(1));
            Assert.That(resultado.First().Producto, Is.Not.Null);
            Assert.That(resultado.First().Producto!.Nombre, Is.EqualTo("Producto Fav"));
        }

        [Test]
        public async Task GetByUserIdAsync_NoDebeRetornarFavoritosDeOtroUsuario()
        {
            // Crear otro usuario
            var otroUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "otro@test.com",
                NormalizedUserName = "OTRO@TEST.COM",
                Email = "otro@test.com",
                NormalizedEmail = "OTRO@TEST.COM",
                Nombre = "Otro",
                Apellidos = "User",
                Role = Role.User
            };
            Context.Users.Add(otroUser);
            Context.Favoritos.Add(new Favorito { UserId = otroUser.Id, ProductoId = _testProductoId });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByUserIdAsync(_testUserId);

            Assert.That(resultado, Is.Empty);
        }

        // ==========================================
        // GetByIdAsync
        // ==========================================

        [Test]
        public async Task GetByIdAsync_SiExiste_DebeRetornarFavorito()
        {
            var favorito = new Favorito { UserId = _testUserId, ProductoId = _testProductoId };
            Context.Favoritos.Add(favorito);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(favorito.Id);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.Id, Is.EqualTo(favorito.Id));
        }

        [Test]
        public async Task GetByIdAsync_SiNoExiste_DebeRetornarNull()
        {
            var resultado = await _repository.GetByIdAsync(9999);

            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // GetByProductAndUserAsync
        // ==========================================

        [Test]
        public async Task GetByProductAndUserAsync_SiExiste_DebeRetornarFavorito()
        {
            Context.Favoritos.Add(new Favorito { UserId = _testUserId, ProductoId = _testProductoId });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByProductAndUserAsync(_testProductoId, _testUserId);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.ProductoId, Is.EqualTo(_testProductoId));
        }

        [Test]
        public async Task GetByProductAndUserAsync_SiNoExiste_DebeRetornarNull()
        {
            var resultado = await _repository.GetByProductAndUserAsync(9999, _testUserId);

            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // AddAsync
        // ==========================================

        [Test]
        public async Task AddAsync_DebeGuardarFavorito()
        {
            var favorito = new Favorito { UserId = _testUserId, ProductoId = _testProductoId };

            await _repository.AddAsync(favorito);

            Assert.That(favorito.Id, Is.GreaterThan(0));

            var encontrado = await Context.Favoritos.FindAsync(favorito.Id);
            Assert.That(encontrado, Is.Not.Null);
        }

        // ==========================================
        // DeleteAsync
        // ==========================================

        [Test]
        public async Task DeleteAsync_SiExiste_DebeEliminarFavorito()
        {
            var favorito = new Favorito { UserId = _testUserId, ProductoId = _testProductoId };
            Context.Favoritos.Add(favorito);
            await Context.SaveChangesAsync();

            await _repository.DeleteAsync(favorito.Id);

            var encontrado = await Context.Favoritos.FindAsync(favorito.Id);
            Assert.That(encontrado, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_SiNoExiste_NoDebeLanzarError()
        {
            Assert.DoesNotThrowAsync(async () => await _repository.DeleteAsync(9999));
        }
    }
}
