using PandaBack.Dtos.Auth;
using PandaBack.Mappers;
using PandaBack.Models;

namespace Tests.Mappers
{
    public class UserMapperTest
    {
        [Test]
        public void ToDto_DebeMapearTodosLosCampos()
        {
            var user = new User
            {
                Id = "user-id-123",
                Nombre = "Juan",
                Apellidos = "García López",
                Email = "juan@test.com",
                Avatar = "https://example.com/avatar.jpg",
                Role = Role.Admin,
                FechaAlta = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc)
            };

            var dto = user.ToDto();

            Assert.That(dto.Id, Is.EqualTo("user-id-123"));
            Assert.That(dto.Nombre, Is.EqualTo("Juan"));
            Assert.That(dto.Apellidos, Is.EqualTo("García López"));
            Assert.That(dto.Email, Is.EqualTo("juan@test.com"));
            Assert.That(dto.Avatar, Is.EqualTo("https://example.com/avatar.jpg"));
            Assert.That(dto.Role, Is.EqualTo("Admin"));
        }

        [Test]
        public void ToDto_SinAvatar_DebeSerNull()
        {
            var user = new User
            {
                Id = "user-id-456",
                Nombre = "Ana",
                Apellidos = "Martínez",
                Email = "ana@test.com",
                Avatar = null,
                Role = Role.User
            };

            var dto = user.ToDto();

            Assert.That(dto.Avatar, Is.Null);
            Assert.That(dto.Role, Is.EqualTo("User"));
        }

        [Test]
        public void ToDto_SinEmail_DebeDevolveCadenaVacia()
        {
            var user = new User
            {
                Id = "user-id-789",
                Nombre = "Test",
                Apellidos = "Test",
                Email = null
            };

            var dto = user.ToDto();

            Assert.That(dto.Email, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToModel_DebeCrearUserDesdeRegisterDto()
        {
            var registerDto = new RegisterDto
            {
                Nombre = "Pedro",
                Apellidos = "López",
                Email = "pedro@test.com",
                Password = "Password123"
            };

            var user = registerDto.ToModel();

            Assert.That(user.Nombre, Is.EqualTo("Pedro"));
            Assert.That(user.Apellidos, Is.EqualTo("López"));
            Assert.That(user.Email, Is.EqualTo("pedro@test.com"));
            Assert.That(user.UserName, Is.EqualTo("pedro@test.com"));
            Assert.That(user.Role, Is.EqualTo(Role.User));
        }

        [Test]
        public void ToModel_RolPorDefecto_DebeSerUser()
        {
            var registerDto = new RegisterDto
            {
                Nombre = "Test",
                Apellidos = "Test",
                Email = "test@test.com",
                Password = "Test123"
            };

            var user = registerDto.ToModel();

            Assert.That(user.Role, Is.EqualTo(Role.User));
        }

        [Test]
        public void ToModel_FechaAlta_DebeSerReciente()
        {
            var antes = DateTime.UtcNow;
            var registerDto = new RegisterDto
            {
                Nombre = "Test",
                Apellidos = "Test",
                Email = "test@test.com",
                Password = "Test123"
            };

            var user = registerDto.ToModel();
            var despues = DateTime.UtcNow;

            Assert.That(user.FechaAlta, Is.InRange(antes, despues));
        }
    }
}
