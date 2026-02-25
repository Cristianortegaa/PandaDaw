using PandaBack.Models;

namespace Tests.Models
{
    public class ProductoModelTest
    {
        [Test]
        public void HasStock_ConStockSuficiente_DebeRetornarTrue()
        {
            var producto = new Producto { Nombre = "Test", Stock = 10, IsDeleted = false };

            Assert.That(producto.HasStock(5), Is.True);
        }

        [Test]
        public void HasStock_ConStockExacto_DebeRetornarTrue()
        {
            var producto = new Producto { Nombre = "Test", Stock = 5, IsDeleted = false };

            Assert.That(producto.HasStock(5), Is.True);
        }

        [Test]
        public void HasStock_ConStockInsuficiente_DebeRetornarFalse()
        {
            var producto = new Producto { Nombre = "Test", Stock = 3, IsDeleted = false };

            Assert.That(producto.HasStock(5), Is.False);
        }

        [Test]
        public void HasStock_ProductoEliminado_DebeRetornarFalse()
        {
            var producto = new Producto { Nombre = "Test", Stock = 100, IsDeleted = true };

            Assert.That(producto.HasStock(1), Is.False);
        }

        [Test]
        public void HasStock_StockCero_DebeRetornarFalse()
        {
            var producto = new Producto { Nombre = "Test", Stock = 0, IsDeleted = false };

            Assert.That(producto.HasStock(1), Is.False);
        }

        [Test]
        public void ReduceStock_ConStockSuficiente_DebeReducirCorrectamente()
        {
            var producto = new Producto { Nombre = "Test", Stock = 10, IsDeleted = false };

            producto.ReduceStock(3);

            Assert.That(producto.Stock, Is.EqualTo(7));
        }

        [Test]
        public void ReduceStock_ConStockExacto_DebeQuedarEnCero()
        {
            var producto = new Producto { Nombre = "Test", Stock = 5, IsDeleted = false };

            producto.ReduceStock(5);

            Assert.That(producto.Stock, Is.EqualTo(0));
        }

        [Test]
        public void ReduceStock_SinStockSuficiente_DebeLanzarExcepcion()
        {
            var producto = new Producto { Nombre = "Auriculares", Stock = 2, IsDeleted = false };

            Assert.Throws<InvalidOperationException>(() => producto.ReduceStock(5));
        }

        [Test]
        public void ReduceStock_ProductoEliminado_DebeLanzarExcepcion()
        {
            var producto = new Producto { Nombre = "Auriculares", Stock = 10, IsDeleted = true };

            Assert.Throws<InvalidOperationException>(() => producto.ReduceStock(1));
        }

        [Test]
        public void IncreaseStock_DebeIncrementarCorrectamente()
        {
            var producto = new Producto { Nombre = "Test", Stock = 5 };

            producto.IncreaseStock(10);

            Assert.That(producto.Stock, Is.EqualTo(15));
        }

        [Test]
        public void IncreaseStock_DesdeStockCero_DebeIncrementar()
        {
            var producto = new Producto { Nombre = "Test", Stock = 0 };

            producto.IncreaseStock(3);

            Assert.That(producto.Stock, Is.EqualTo(3));
        }

        [Test]
        public void FechaAlta_PorDefecto_DebeSerUtcNow()
        {
            var antes = DateTime.UtcNow;
            var producto = new Producto();
            var despues = DateTime.UtcNow;

            Assert.That(producto.FechaAlta, Is.InRange(antes, despues));
        }

        [Test]
        public void IsDeleted_PorDefecto_DebeSeFalse()
        {
            var producto = new Producto();

            Assert.That(producto.IsDeleted, Is.False);
        }
    }
}
