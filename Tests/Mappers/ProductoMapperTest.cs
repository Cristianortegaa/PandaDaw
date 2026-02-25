using PandaBack.Mappers;
using PandaBack.Models;

namespace Tests.Mappers
{
    public class ProductoMapperTest
    {
        [Test]
        public void ToDto_DebeMapearTodosLosCampos()
        {
            var producto = new Producto
            {
                Id = 1,
                Nombre = "Auriculares Sony",
                Precio = 349.00m,
                Stock = 25,
                Imagen = "https://example.com/imagen.jpg",
                Category = Categoria.Audio,
                IsDeleted = false
            };

            var dto = producto.ToDto();

            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.Nombre, Is.EqualTo("Auriculares Sony"));
            Assert.That(dto.Precio, Is.EqualTo(349.00m));
            Assert.That(dto.Stock, Is.EqualTo(25));
            Assert.That(dto.Imagen, Is.EqualTo("https://example.com/imagen.jpg"));
            Assert.That(dto.Categoria, Is.EqualTo("Audio"));
            Assert.That(dto.IsDeleted, Is.False);
        }

        [Test]
        public void ToDto_SinImagen_DebeUsarPlaceholder()
        {
            var producto = new Producto
            {
                Id = 2,
                Nombre = "Producto sin imagen",
                Precio = 10,
                Stock = 5,
                Imagen = null,
                Category = Categoria.Gaming
            };

            var dto = producto.ToDto();

            Assert.That(dto.Imagen, Is.EqualTo("https://placehold.net/600x400.png"));
        }

        [Test]
        public void ToDto_ProductoEliminado_DebeReflejarIsDeleted()
        {
            var producto = new Producto
            {
                Id = 3,
                Nombre = "Eliminado",
                IsDeleted = true,
                Category = Categoria.Laptops
            };

            var dto = producto.ToDto();

            Assert.That(dto.IsDeleted, Is.True);
        }

        [Test]
        public void ToDto_CadaCategoria_DebeConvertirseCorrectamente()
        {
            foreach (Categoria cat in Enum.GetValues<Categoria>())
            {
                var producto = new Producto { Category = cat };
                var dto = producto.ToDto();
                Assert.That(dto.Categoria, Is.EqualTo(cat.ToString()));
            }
        }
    }
}
