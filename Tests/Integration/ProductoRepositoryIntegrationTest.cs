using Microsoft.Extensions.Logging;
using Moq;
using PandaBack.Models;
using PandaBack.Repositories;

namespace Tests.Integration
{
    /// <summary>
    /// Tests de integración de ProductoRepository con PostgreSQL real (TestContainers).
    /// Verifica que las consultas y operaciones CRUD funcionan contra la base de datos.
    /// </summary>
    [TestFixture]
    public class ProductoRepositoryIntegrationTest : PostgresIntegrationTestBase
    {
        private ProductoRepository _repository = null!;

        [SetUp]
        public void PrepararRepositorio()
        {
            var logger = new Mock<ILogger<ProductoRepository>>();
            _repository = new ProductoRepository(Context, logger.Object);
        }

        // ==========================================
        // GetAllAsync
        // ==========================================

        [Test]
        public async Task GetAllAsync_SinProductos_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetAllAsync();

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetAllAsync_ConProductos_DebeRetornarSoloActivos()
        {
            // PREPARAR
            Context.Productos.Add(new Producto
            {
                Nombre = "Activo 1", Precio = 10, Stock = 5,
                Category = Categoria.Audio, IsDeleted = false
            });
            Context.Productos.Add(new Producto
            {
                Nombre = "Activo 2", Precio = 20, Stock = 3,
                Category = Categoria.Gaming, IsDeleted = false
            });
            Context.Productos.Add(new Producto
            {
                Nombre = "Eliminado", Precio = 30, Stock = 1,
                Category = Categoria.Laptops, IsDeleted = true
            });
            await Context.SaveChangesAsync();

            // ACTUAR
            var resultado = await _repository.GetAllAsync();

            // COMPROBAR
            Assert.That(resultado.Count(), Is.EqualTo(2));
            Assert.That(resultado.All(p => !p.IsDeleted), Is.True);
        }

        // ==========================================
        // GetByIdAsync
        // ==========================================

        [Test]
        public async Task GetByIdAsync_SiExisteYNoEliminado_DebeRetornarProducto()
        {
            var producto = new Producto
            {
                Nombre = "Test", Precio = 99.99m, Stock = 10,
                Category = Categoria.Smartphones, IsDeleted = false
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(producto.Id);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.Nombre, Is.EqualTo("Test"));
            Assert.That(resultado.Precio, Is.EqualTo(99.99m));
        }

        [Test]
        public async Task GetByIdAsync_SiEstaEliminado_DebeRetornarNull()
        {
            var producto = new Producto
            {
                Nombre = "Eliminado", Precio = 50, Stock = 5,
                Category = Categoria.Audio, IsDeleted = true
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByIdAsync(producto.Id);

            Assert.That(resultado, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_SiNoExiste_DebeRetornarNull()
        {
            var resultado = await _repository.GetByIdAsync(9999);

            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // GetByCategoryAsync
        // ==========================================

        [Test]
        public async Task GetByCategoryAsync_DebeRetornarSoloDeEsaCategoria()
        {
            Context.Productos.Add(new Producto
            {
                Nombre = "Auriculares", Precio = 100, Stock = 5,
                Category = Categoria.Audio, IsDeleted = false
            });
            Context.Productos.Add(new Producto
            {
                Nombre = "Altavoz", Precio = 50, Stock = 10,
                Category = Categoria.Audio, IsDeleted = false
            });
            Context.Productos.Add(new Producto
            {
                Nombre = "Laptop", Precio = 1000, Stock = 2,
                Category = Categoria.Laptops, IsDeleted = false
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByCategoryAsync(Categoria.Audio);

            Assert.That(resultado.Count(), Is.EqualTo(2));
            Assert.That(resultado.All(p => p.Category == Categoria.Audio), Is.True);
        }

        [Test]
        public async Task GetByCategoryAsync_NoDebeRetornarEliminados()
        {
            Context.Productos.Add(new Producto
            {
                Nombre = "Audio Eliminado", Precio = 100, Stock = 5,
                Category = Categoria.Audio, IsDeleted = true
            });
            await Context.SaveChangesAsync();

            var resultado = await _repository.GetByCategoryAsync(Categoria.Audio);

            Assert.That(resultado, Is.Empty);
        }

        [Test]
        public async Task GetByCategoryAsync_CategoriaVacia_DebeRetornarListaVacia()
        {
            var resultado = await _repository.GetByCategoryAsync(Categoria.Imagen);

            Assert.That(resultado, Is.Empty);
        }

        // ==========================================
        // AddAsync
        // ==========================================

        [Test]
        public async Task AddAsync_DebeGuardarProductoEnBaseDeDatos()
        {
            var producto = new Producto
            {
                Nombre = "Nuevo Producto", Precio = 75, Stock = 20,
                Category = Categoria.Gaming, IsDeleted = false
            };

            await _repository.AddAsync(producto);

            Assert.That(producto.Id, Is.GreaterThan(0));

            var encontrado = await Context.Productos.FindAsync(producto.Id);
            Assert.That(encontrado, Is.Not.Null);
            Assert.That(encontrado!.Nombre, Is.EqualTo("Nuevo Producto"));
        }

        // ==========================================
        // UpdateAsync
        // ==========================================

        [Test]
        public async Task UpdateAsync_DebeActualizarProducto()
        {
            var producto = new Producto
            {
                Nombre = "Antiguo", Precio = 50, Stock = 5,
                Category = Categoria.Audio, IsDeleted = false
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            // Modificar
            producto.Nombre = "Actualizado";
            producto.Precio = 75;
            await _repository.UpdateAsync(producto);

            // Verificar con un nuevo contexto de lectura
            var encontrado = await Context.Productos.FindAsync(producto.Id);
            Assert.That(encontrado!.Nombre, Is.EqualTo("Actualizado"));
            Assert.That(encontrado.Precio, Is.EqualTo(75));
        }

        // ==========================================
        // DeleteAsync
        // ==========================================

        [Test]
        public async Task DeleteAsync_SiExiste_DebeEliminarProducto()
        {
            var producto = new Producto
            {
                Nombre = "A Eliminar", Precio = 10, Stock = 1,
                Category = Categoria.Imagen, IsDeleted = false
            };
            Context.Productos.Add(producto);
            await Context.SaveChangesAsync();

            await _repository.DeleteAsync(producto.Id);

            var encontrado = await Context.Productos.FindAsync(producto.Id);
            Assert.That(encontrado, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_SiNoExiste_NoDebeLanzarError()
        {
            // No debe lanzar excepción
            Assert.DoesNotThrowAsync(async () => await _repository.DeleteAsync(9999));
        }
    }
}
