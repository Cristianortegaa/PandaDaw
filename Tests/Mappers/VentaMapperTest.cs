using PandaBack.Mappers;
using PandaBack.Models;

namespace Tests.Mappers
{
    public class VentaMapperTest
    {
        [Test]
        public void ToDto_ConUsuarioYLineas_DebeMapearTodosLosCampos()
        {
            var user = new User
            {
                Id = "user-1",
                Nombre = "Juan",
                Apellidos = "García",
                Email = "juan@test.com"
            };

            var producto = new Producto
            {
                Id = 1,
                Nombre = "Auriculares",
                Imagen = "img.jpg"
            };

            var venta = new Venta
            {
                Id = 100,
                FechaCompra = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
                Total = 200,
                Estado = EstadoPedido.Procesando,
                UserId = "user-1",
                User = user,
                Lineas = new List<LineaVenta>
                {
                    new LineaVenta
                    {
                        ProductoId = 1,
                        Cantidad = 2,
                        PrecioUnitario = 100,
                        Producto = producto
                    }
                }
            };

            var dto = venta.ToDto();

            Assert.That(dto.Id, Is.EqualTo(100));
            Assert.That(dto.Total, Is.EqualTo(200));
            Assert.That(dto.Estado, Is.EqualTo("Procesando"));
            Assert.That(dto.UsuarioId, Is.EqualTo("user-1"));
            Assert.That(dto.UsuarioNombre, Is.EqualTo("Juan García"));
            Assert.That(dto.UsuarioEmail, Is.EqualTo("juan@test.com"));
            Assert.That(dto.Lineas.Count, Is.EqualTo(1));
            Assert.That(dto.Lineas[0].ProductoNombre, Is.EqualTo("Auriculares"));
            Assert.That(dto.Lineas[0].PrecioUnitario, Is.EqualTo(100));
            Assert.That(dto.Lineas[0].Cantidad, Is.EqualTo(2));
            Assert.That(dto.Lineas[0].Subtotal, Is.EqualTo(200));
        }

        [Test]
        public void ToDto_SinUsuario_DebeUsarValoresPorDefecto()
        {
            var venta = new Venta
            {
                Id = 200,
                UserId = "user-2",
                User = null,
                Estado = EstadoPedido.Pendiente,
                Lineas = new List<LineaVenta>()
            };

            var dto = venta.ToDto();

            Assert.That(dto.UsuarioNombre, Is.EqualTo("Usuario Desconocido"));
            Assert.That(dto.UsuarioEmail, Is.EqualTo(""));
        }

        [Test]
        public void ToDto_CadaEstado_DebeConvertirseCorrectamente()
        {
            foreach (EstadoPedido estado in Enum.GetValues<EstadoPedido>())
            {
                var venta = new Venta
                {
                    Estado = estado,
                    UserId = "u",
                    Lineas = new List<LineaVenta>()
                };
                var dto = venta.ToDto();
                Assert.That(dto.Estado, Is.EqualTo(estado.ToString()));
            }
        }

        [Test]
        public void LineaVenta_ToDto_ConProducto_DebeMapearCorrectamente()
        {
            var producto = new Producto { Id = 1, Nombre = "Test", Imagen = "test.jpg" };
            var linea = new LineaVenta
            {
                ProductoId = 1,
                Cantidad = 3,
                PrecioUnitario = 25,
                Producto = producto
            };

            var dto = linea.ToDto();

            Assert.That(dto.ProductoId, Is.EqualTo(1));
            Assert.That(dto.ProductoNombre, Is.EqualTo("Test"));
            Assert.That(dto.ProductoImagen, Is.EqualTo("test.jpg"));
            Assert.That(dto.Cantidad, Is.EqualTo(3));
            Assert.That(dto.PrecioUnitario, Is.EqualTo(25));
            Assert.That(dto.Subtotal, Is.EqualTo(75));
        }

        [Test]
        public void LineaVenta_ToDto_SinProducto_DebeUsarValoresPorDefecto()
        {
            var linea = new LineaVenta
            {
                ProductoId = 99,
                Cantidad = 1,
                PrecioUnitario = 10,
                Producto = null
            };

            var dto = linea.ToDto();

            Assert.That(dto.ProductoNombre, Is.EqualTo("Producto no disponible"));
            Assert.That(dto.ProductoImagen, Is.EqualTo("https://via.placeholder.com/50"));
        }
    }
}
