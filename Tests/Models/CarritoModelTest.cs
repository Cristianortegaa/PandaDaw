using PandaBack.Models;

namespace Tests.Models
{
    public class CarritoModelTest
    {
        [Test]
        public void Total_SinLineas_DebeSeCero()
        {
            var carrito = new Carrito { UserId = "user-1" };

            Assert.That(carrito.Total, Is.EqualTo(0));
        }

        [Test]
        public void TotalItems_SinLineas_DebeSeCero()
        {
            var carrito = new Carrito { UserId = "user-1" };

            Assert.That(carrito.TotalItems, Is.EqualTo(0));
        }

        [Test]
        public void Total_ConLineas_DebeCalcularCorrectamente()
        {
            var carrito = new Carrito
            {
                UserId = "user-1",
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito
                    {
                        ProductoId = 1,
                        Cantidad = 2,
                        Producto = new Producto { Id = 1, Precio = 100, Stock = 10, IsDeleted = false }
                    },
                    new LineaCarrito
                    {
                        ProductoId = 2,
                        Cantidad = 1,
                        Producto = new Producto { Id = 2, Precio = 50, Stock = 5, IsDeleted = false }
                    }
                }
            };

            Assert.That(carrito.Total, Is.EqualTo(250)); // 2*100 + 1*50
        }

        [Test]
        public void TotalItems_ConLineas_DebeSumarCantidades()
        {
            var carrito = new Carrito
            {
                UserId = "user-1",
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { Cantidad = 2 },
                    new LineaCarrito { Cantidad = 3 }
                }
            };

            Assert.That(carrito.TotalItems, Is.EqualTo(5));
        }

        [Test]
        public void AddLineaCarrito_NuevoProducto_DebeAgregarLinea()
        {
            var carrito = new Carrito { UserId = "user-1" };
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 10, IsDeleted = false };

            var linea = new LineaCarrito { ProductoId = 1, Cantidad = 2, Producto = producto };
            carrito.AddLineaCarrito(linea);

            Assert.That(carrito.LineasCarrito.Count, Is.EqualTo(1));
            Assert.That(carrito.LineasCarrito.First().Cantidad, Is.EqualTo(2));
        }

        [Test]
        public void AddLineaCarrito_ProductoExistente_DebeIncrementarCantidad()
        {
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 20, IsDeleted = false };
            var carrito = new Carrito
            {
                UserId = "user-1",
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 1, Cantidad = 2, Producto = producto }
                }
            };

            var nuevaLinea = new LineaCarrito { ProductoId = 1, Cantidad = 3, Producto = producto };
            carrito.AddLineaCarrito(nuevaLinea);

            Assert.That(carrito.LineasCarrito.Count, Is.EqualTo(1));
            Assert.That(carrito.LineasCarrito.First().Cantidad, Is.EqualTo(5));
        }

        [Test]
        public void AddLineaCarrito_SinStockSuficienteNuevo_DebeLanzarExcepcion()
        {
            var carrito = new Carrito { UserId = "user-1" };
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 1, IsDeleted = false };

            var linea = new LineaCarrito { ProductoId = 1, Cantidad = 5, Producto = producto };

            Assert.Throws<InvalidOperationException>(() => carrito.AddLineaCarrito(linea));
        }

        [Test]
        public void AddLineaCarrito_SinStockSuficienteExistente_DebeLanzarExcepcion()
        {
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 5, IsDeleted = false };
            var carrito = new Carrito
            {
                UserId = "user-1",
                LineasCarrito = new List<LineaCarrito>
                {
                    new LineaCarrito { ProductoId = 1, Cantidad = 3, Producto = producto }
                }
            };

            var nuevaLinea = new LineaCarrito { ProductoId = 1, Cantidad = 5, Producto = producto };

            Assert.Throws<InvalidOperationException>(() => carrito.AddLineaCarrito(nuevaLinea));
        }

        [Test]
        public void RemoveLineaCarrito_DebeEliminarLinea()
        {
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 10, IsDeleted = false };
            var linea = new LineaCarrito { ProductoId = 1, Cantidad = 2, Producto = producto };
            var carrito = new Carrito
            {
                UserId = "user-1",
                LineasCarrito = new List<LineaCarrito> { linea }
            };

            carrito.RemoveLineaCarrito(linea);

            Assert.That(carrito.LineasCarrito.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveLineaCarrito_DebeActualizarUpdatedAt()
        {
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 10, IsDeleted = false };
            var linea = new LineaCarrito { ProductoId = 1, Cantidad = 2, Producto = producto };
            var carrito = new Carrito
            {
                UserId = "user-1",
                LineasCarrito = new List<LineaCarrito> { linea },
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var antes = DateTime.UtcNow;
            carrito.RemoveLineaCarrito(linea);

            Assert.That(carrito.UpdatedAt, Is.GreaterThanOrEqualTo(antes));
        }

        [Test]
        public void AddLineaCarrito_DebeActualizarUpdatedAt()
        {
            var carrito = new Carrito
            {
                UserId = "user-1",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            var producto = new Producto { Id = 1, Nombre = "Test", Precio = 50, Stock = 10, IsDeleted = false };
            var linea = new LineaCarrito { ProductoId = 1, Cantidad = 1, Producto = producto };

            var antes = DateTime.UtcNow;
            carrito.AddLineaCarrito(linea);

            Assert.That(carrito.UpdatedAt, Is.GreaterThanOrEqualTo(antes));
        }

        [Test]
        public void Carrito_PorDefecto_LineasDebeSerColeccionVacia()
        {
            var carrito = new Carrito();

            Assert.That(carrito.LineasCarrito, Is.Not.Null);
            Assert.That(carrito.LineasCarrito.Count, Is.EqualTo(0));
        }
    }
}
