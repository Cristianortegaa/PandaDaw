using PandaBack.Models;

namespace Tests.Models
{
    public class LineaCarritoModelTest
    {
        [Test]
        public void Subtotal_ConProducto_DebeCalcularCorrectamente()
        {
            var linea = new LineaCarrito
            {
                Cantidad = 3,
                Producto = new Producto { Precio = 25.50m }
            };

            Assert.That(linea.Subtotal, Is.EqualTo(76.50m));
        }

        [Test]
        public void Subtotal_SinProducto_DebeSeCero()
        {
            var linea = new LineaCarrito { Cantidad = 3, Producto = null };

            Assert.That(linea.Subtotal, Is.EqualTo(0));
        }

        [Test]
        public void IncrementQuantity_DebeIncrementarCantidad()
        {
            var linea = new LineaCarrito { Cantidad = 2 };

            linea.IncrementQuantity(3);

            Assert.That(linea.Cantidad, Is.EqualTo(5));
        }

        [Test]
        public void Cantidad_PorDefecto_DebeSerUno()
        {
            var linea = new LineaCarrito();

            Assert.That(linea.Cantidad, Is.EqualTo(1));
        }
    }

    public class LineaVentaModelTest
    {
        [Test]
        public void Subtotal_DebeCalcularCorrectamente()
        {
            var linea = new LineaVenta
            {
                Cantidad = 4,
                PrecioUnitario = 15.00m
            };

            Assert.That(linea.Subtotal, Is.EqualTo(60.00m));
        }

        [Test]
        public void Subtotal_CantidadUno_DebeSerIgualAlPrecio()
        {
            var linea = new LineaVenta
            {
                Cantidad = 1,
                PrecioUnitario = 99.99m
            };

            Assert.That(linea.Subtotal, Is.EqualTo(99.99m));
        }
    }
}
