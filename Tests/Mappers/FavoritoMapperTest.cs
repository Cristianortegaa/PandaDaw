using PandaBack.Mappers;
using PandaBack.Models;

namespace Tests.Mappers
{
    public class FavoritoMapperTest
    {
        [Test]
        public void ToDto_ConProducto_DebeMapearTodosLosCampos()
        {
            var producto = new Producto
            {
                Id = 5,
                Nombre = "Auriculares",
                Imagen = "https://example.com/img.jpg",
                Precio = 299.00m,
                Stock = 15,
                Category = Categoria.Audio
            };

            var favorito = new Favorito
            {
                Id = 10,
                UserId = "user-1",
                ProductoId = 5,
                Producto = producto,
                CreatedAt = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc)
            };

            var dto = favorito.ToDto();

            Assert.That(dto.Id, Is.EqualTo(10));
            Assert.That(dto.AgregadoEl, Is.EqualTo(new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(dto.ProductoId, Is.EqualTo(5));
            Assert.That(dto.ProductoNombre, Is.EqualTo("Auriculares"));
            Assert.That(dto.ProductoImagen, Is.EqualTo("https://example.com/img.jpg"));
            Assert.That(dto.ProductoPrecio, Is.EqualTo(299.00m));
            Assert.That(dto.ProductoStock, Is.EqualTo(15));
            Assert.That(dto.ProductoCategoria, Is.EqualTo("Audio"));
        }

        [Test]
        public void ToDto_SinProducto_DebeUsarValoresPorDefecto()
        {
            var favorito = new Favorito
            {
                Id = 20,
                UserId = "user-2",
                ProductoId = 99,
                Producto = null
            };

            var dto = favorito.ToDto();

            Assert.That(dto.ProductoNombre, Is.EqualTo("Producto no disponible"));
            Assert.That(dto.ProductoImagen, Is.EqualTo("https://via.placeholder.com/150"));
            Assert.That(dto.ProductoPrecio, Is.EqualTo(0));
            Assert.That(dto.ProductoStock, Is.EqualTo(0));
            Assert.That(dto.ProductoCategoria, Is.EqualTo("Sin Categoría"));
        }
    }
}
