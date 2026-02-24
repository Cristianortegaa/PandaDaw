using PandaBack.Dtos.Valoraciones;
using PandaBack.Mappers;
using PandaBack.Models;

namespace Tests.Mappers
{
    public class ValoracionMapperTest
    {
        [Test]
        public void ToDto_ConUsuarioYProducto_DebeMapearTodosLosCampos()
        {
            var user = new User
            {
                Id = "user-1",
                Nombre = "Juan",
                Apellidos = "García",
                Avatar = "https://example.com/avatar.jpg"
            };

            var producto = new Producto
            {
                Id = 5,
                Nombre = "Auriculares Sony"
            };

            var valoracion = new Valoracion
            {
                Id = 10,
                Estrellas = 4,
                Resena = "Muy buen producto",
                CreatedAt = new DateTime(2025, 3, 10, 12, 0, 0, DateTimeKind.Utc),
                UserId = "user-1",
                User = user,
                ProductoId = 5,
                Producto = producto
            };

            var dto = valoracion.ToDto();

            Assert.That(dto.Id, Is.EqualTo(10));
            Assert.That(dto.Estrellas, Is.EqualTo(4));
            Assert.That(dto.Resena, Is.EqualTo("Muy buen producto"));
            Assert.That(dto.Fecha, Is.EqualTo(new DateTime(2025, 3, 10, 12, 0, 0, DateTimeKind.Utc)));
            Assert.That(dto.UsuarioId, Is.EqualTo("user-1"));
            Assert.That(dto.UsuarioNombre, Is.EqualTo("Juan García"));
            Assert.That(dto.UsuarioAvatar, Is.EqualTo("https://example.com/avatar.jpg"));
            Assert.That(dto.ProductoId, Is.EqualTo(5));
            Assert.That(dto.ProductoNombre, Is.EqualTo("Auriculares Sony"));
        }

        [Test]
        public void ToDto_SinUsuario_DebeUsarValoresPorDefecto()
        {
            var valoracion = new Valoracion
            {
                Id = 20,
                Estrellas = 3,
                Resena = "Ok",
                UserId = "user-2",
                User = null,
                ProductoId = 1,
                Producto = null
            };

            var dto = valoracion.ToDto();

            Assert.That(dto.UsuarioNombre, Is.EqualTo("Usuario Anónimo"));
            Assert.That(dto.UsuarioAvatar, Is.EqualTo(""));
            Assert.That(dto.ProductoNombre, Is.EqualTo(""));
        }

        [Test]
        public void ToDto_UsuarioSinAvatar_DebeDevolveCadenaVacia()
        {
            var user = new User { Id = "user-3", Nombre = "Ana", Apellidos = "López", Avatar = null };
            var valoracion = new Valoracion
            {
                Id = 30,
                UserId = "user-3",
                User = user,
                Estrellas = 5,
                Resena = "Excelente"
            };

            var dto = valoracion.ToDto();

            Assert.That(dto.UsuarioAvatar, Is.EqualTo(""));
        }

        [Test]
        public void ToModel_DebeCrearValoracionDesdeDto()
        {
            var dto = new CreateValoracionDto
            {
                ProductoId = 5,
                Estrellas = 4,
                Resena = "Buen producto"
            };

            var antes = DateTime.UtcNow;
            var valoracion = dto.ToModel("user-1");
            var despues = DateTime.UtcNow;

            Assert.That(valoracion.ProductoId, Is.EqualTo(5));
            Assert.That(valoracion.UserId, Is.EqualTo("user-1"));
            Assert.That(valoracion.Estrellas, Is.EqualTo(4));
            Assert.That(valoracion.Resena, Is.EqualTo("Buen producto"));
            Assert.That(valoracion.CreatedAt, Is.InRange(antes, despues));
        }
    }
}
