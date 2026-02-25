using PandaBack.Mappers;
using PandaBack.Models;

namespace Tests.Mappers
{
    public class CarritoMapperTest
    {
        [Test]
        public void ToDto_CarritoVacio_DebeMapearCorrectamente()
        {
            var carrito = new Carrito
            {
                Id = 1,
                UserId = "user-123",
                LineasCarrito = new List<LineaCarrito>()
            };

            var dto = carrito.ToDto();

            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.UsuarioId, Is.EqualTo("user-123"));
            Assert.That(dto.Lineas, Is.Empty);
            Assert.That(dto.Total, Is.EqualTo(0));
            Assert.That(dto.TotalItems, Is.EqualTo(0));
        }

        [Test]
        public void ToDto_ConLineas_DebeMapearLineasYTotales()
        {
            var producto1 = new Producto { Id = 1, Nombre = "Producto A", Precio = 100, Imagen = "img1.jpg" };
            var producto2 = new Producto { Id = 2, Nombre = "Producto B", Precio = 50, Imagen = "img2.jpg" };

            var carrito = new Carrito
            {
                Id = 10,
                UserId = "user-456",
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 1, Cantidad = 2, Producto = producto1 },
                    new LineaCarrito { ProductoId = 2, Cantidad = 3, Producto = producto2 }
                }
            };

            var dto = carrito.ToDto();

            Assert.That(dto.Lineas.Count, Is.EqualTo(2));
            Assert.That(dto.Total, Is.EqualTo(350)); // 2*100 + 3*50
            Assert.That(dto.TotalItems, Is.EqualTo(5));

            Assert.That(dto.Lineas[0].ProductoNombre, Is.EqualTo("Producto A"));
            Assert.That(dto.Lineas[0].PrecioUnitario, Is.EqualTo(100));
            Assert.That(dto.Lineas[0].Cantidad, Is.EqualTo(2));
            Assert.That(dto.Lineas[0].Subtotal, Is.EqualTo(200));
        }

        [Test]
        public void ToDto_ConProductoNull_DebeUsarValoresPorDefecto()
        {
            var carrito = new Carrito
            {
                Id = 20,
                UserId = "user-789",
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 1, Cantidad = 1, Producto = null }
                }
            };

            var dto = carrito.ToDto();

            Assert.That(dto.Lineas[0].ProductoNombre, Is.EqualTo("Producto Desconocido"));
            Assert.That(dto.Lineas[0].ProductoImagen, Is.EqualTo(""));
            Assert.That(dto.Lineas[0].PrecioUnitario, Is.EqualTo(0));
        }
    }
}
