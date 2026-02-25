using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PandaBack.Dtos.Auth;
using PandaBack.Models;
using PandaBack.Repositories.Auth;
using PandaBack.Services.Auth;

namespace Tests.Services
{
    public class AuthServiceTest
    {
        private AuthService _service;
        private Mock<IAuthRepository> _repoAuthFalso;
        private Mock<TokenService> _tokenServiceFalso;
        private Mock<ILogger<AuthService>> _loggerFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _repoAuthFalso = new Mock<IAuthRepository>();
            _loggerFalso = new Mock<ILogger<AuthService>>();

            // TokenService necesita IConfiguration, así que lo mockeamos
            var configFalso = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            configFalso.Setup(c => c["Jwt:Key"]).Returns("UnaClaveSuperSeguraDe32CaracteresMinimoParaHMAC");
            configFalso.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            configFalso.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
            configFalso.Setup(c => c["Jwt:ExpireInMinutes"]).Returns("60");

            var tokenService = new TokenService(configFalso.Object);

            _service = new AuthService(_repoAuthFalso.Object, tokenService, _loggerFalso.Object);
        }

        // ==========================================
        // REGISTRO
        // ==========================================

        [Test]
        public async Task Register_ConDatosCorrectos_DebeTenerExito()
        {
            // PREPARAR
            var dto = new RegisterDto
            {
                Nombre = "Juan",
                Apellidos = "García",
                Email = "juan@test.com",
                Password = "Password123"
            };

            _repoAuthFalso.Setup(r => r.RegisterAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // ACTUAR
            var resultado = await _service.RegisterAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Email, Is.EqualTo("juan@test.com"));
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Juan"));
            Assert.That(resultado.Value.Apellidos, Is.EqualTo("García"));
        }

        [Test]
        public async Task Register_ConErrorDeIdentity_DebeFallar()
        {
            // PREPARAR
            var dto = new RegisterDto
            {
                Nombre = "Test",
                Apellidos = "Test",
                Email = "test@test.com",
                Password = "123"
            };

            var errores = new[]
            {
                new IdentityError { Code = "PasswordTooShort", Description = "La contraseña es demasiado corta" }
            };

            _repoAuthFalso.Setup(r => r.RegisterAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(errores));

            // ACTUAR
            var resultado = await _service.RegisterAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("demasiado corta"));
        }

        [Test]
        public async Task Register_ConMultiplesErrores_DebeJuntarMensajes()
        {
            // PREPARAR
            var dto = new RegisterDto
            {
                Nombre = "Test",
                Apellidos = "Test",
                Email = "test@test.com",
                Password = "123"
            };

            var errores = new[]
            {
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            };

            _repoAuthFalso.Setup(r => r.RegisterAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(errores));

            // ACTUAR
            var resultado = await _service.RegisterAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("Error 1"));
            Assert.That(resultado.Error, Does.Contain("Error 2"));
        }

        // ==========================================
        // LOGIN
        // ==========================================

        [Test]
        public async Task Login_ConCredencialesCorrectas_DebeDevolverToken()
        {
            // PREPARAR
            var dto = new LoginDto { Email = "juan@test.com", Password = "Password123" };
            var user = new User
            {
                Id = "user-id-123",
                Nombre = "Juan",
                Apellidos = "García",
                Email = "juan@test.com",
                Role = Role.User,
                Avatar = "https://example.com/avatar.jpg"
            };

            _repoAuthFalso.Setup(r => r.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _repoAuthFalso.Setup(r => r.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            // ACTUAR
            var resultado = await _service.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Token, Is.Not.Empty);
            Assert.That(resultado.Value.Id, Is.EqualTo("user-id-123"));
            Assert.That(resultado.Value.Nombre, Is.EqualTo("Juan"));
            Assert.That(resultado.Value.Email, Is.EqualTo("juan@test.com"));
            Assert.That(resultado.Value.Role, Is.EqualTo("User"));
        }

        [Test]
        public async Task Login_ConUsuarioNoExistente_DebeFallar()
        {
            // PREPARAR
            var dto = new LoginDto { Email = "noexiste@test.com", Password = "Password123" };

            _repoAuthFalso.Setup(r => r.FindByEmailAsync(dto.Email)).ReturnsAsync((User)null!);

            // ACTUAR
            var resultado = await _service.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("Credenciales inválidas"));
        }

        [Test]
        public async Task Login_ConPasswordIncorrecta_DebeFallar()
        {
            // PREPARAR
            var dto = new LoginDto { Email = "juan@test.com", Password = "MalaPassword" };
            var user = new User
            {
                Id = "user-id-123",
                Email = "juan@test.com",
                Nombre = "Juan",
                Apellidos = "García"
            };

            _repoAuthFalso.Setup(r => r.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _repoAuthFalso.Setup(r => r.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            // ACTUAR
            var resultado = await _service.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsFailure, Is.True);
            Assert.That(resultado.Error, Does.Contain("Credenciales inválidas"));
        }

        [Test]
        public async Task Login_ConUsuarioAdmin_DebeReflejarRolAdmin()
        {
            // PREPARAR
            var dto = new LoginDto { Email = "admin@test.com", Password = "Admin123" };
            var user = new User
            {
                Id = "admin-id",
                Nombre = "Admin",
                Apellidos = "Sistema",
                Email = "admin@test.com",
                Role = Role.Admin
            };

            _repoAuthFalso.Setup(r => r.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _repoAuthFalso.Setup(r => r.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            // ACTUAR
            var resultado = await _service.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Role, Is.EqualTo("Admin"));
        }

        [Test]
        public async Task Login_SinAvatar_DebeUsarPlaceholder()
        {
            // PREPARAR
            var dto = new LoginDto { Email = "test@test.com", Password = "Pass123" };
            var user = new User
            {
                Id = "user-id",
                Nombre = "Test",
                Apellidos = "User",
                Email = "test@test.com",
                Role = Role.User,
                Avatar = null
            };

            _repoAuthFalso.Setup(r => r.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _repoAuthFalso.Setup(r => r.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            // ACTUAR
            var resultado = await _service.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado.IsSuccess, Is.True);
            Assert.That(resultado.Value.Avatar, Is.EqualTo("https://via.placeholder.com/150"));
        }
    }
}
