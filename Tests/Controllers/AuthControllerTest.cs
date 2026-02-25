using CSharpFunctionalExtensions;
using Moq;
using PandaBack.Dtos.Auth;
using PandaBack.RestController;
using PandaBack.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers
{
    public class AuthControllerTest
    {
        private AuthController _controller;
        private Mock<IAuthService> _serviceFalso;

        [SetUp]
        public void PrepararTodo()
        {
            _serviceFalso = new Mock<IAuthService>();
            _controller = new AuthController(_serviceFalso.Object);
        }

        // ==========================================
        // 1. PRUEBAS DE REGISTRO
        // ==========================================

        [Test]
        public async Task Register_ConDatosCorrectos_DebeDevolver201()
        {
            // PREPARAR
            var dto = new RegisterDto
            {
                Nombre = "Jorge",
                Apellidos = "Martinez",
                Email = "jorge@test.com",
                Password = "password123"
            };
            var respuesta = new UserResponseDto
            {
                Id = "user-id-1",
                Nombre = "Jorge",
                Apellidos = "Martinez",
                Email = "jorge@test.com",
                Role = "User"
            };

            _serviceFalso.Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync(Result.Success(respuesta));

            // ACTUAR
            var resultado = await _controller.RegisterAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<CreatedResult>());
        }

        [Test]
        public async Task Register_ConDatosInvalidos_DebeDevolver400()
        {
            // PREPARAR
            var dto = new RegisterDto
            {
                Nombre = "",
                Apellidos = "",
                Email = "email-invalido",
                Password = "123"
            };

            _serviceFalso.Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync(Result.Failure<UserResponseDto>("Datos de registro inválidos"));

            // ACTUAR
            var resultado = await _controller.RegisterAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Register_ConEmailDuplicado_DebeDevolver400()
        {
            // PREPARAR
            var dto = new RegisterDto
            {
                Nombre = "Jorge",
                Apellidos = "Martinez",
                Email = "jorge@test.com",
                Password = "password123"
            };

            _serviceFalso.Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync(Result.Failure<UserResponseDto>("El email ya está registrado"));

            // ACTUAR
            var resultado = await _controller.RegisterAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ==========================================
        // 2. PRUEBAS DE LOGIN
        // ==========================================

        [Test]
        public async Task Login_ConCredencialesCorrectas_DebeDevolver200()
        {
            // PREPARAR
            var dto = new LoginDto
            {
                Email = "jorge@test.com",
                Password = "password123"
            };
            var respuesta = new LoginResponseDto
            {
                Token = "jwt-token-falso",
                Id = "user-id-1",
                Nombre = "Jorge",
                Apellidos = "Martinez",
                Email = "jorge@test.com",
                Role = "User"
            };

            _serviceFalso.Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(Result.Success(respuesta));

            // ACTUAR
            var resultado = await _controller.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Login_ConCredencialesInvalidas_DebeDevolver401()
        {
            // PREPARAR
            var dto = new LoginDto
            {
                Email = "jorge@test.com",
                Password = "contraseña-mala"
            };

            _serviceFalso.Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(Result.Failure<LoginResponseDto>("Credenciales inválidas"));

            // ACTUAR
            var resultado = await _controller.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Login_ConCamposVacios_DebeDevolver401()
        {
            // PREPARAR
            var dto = new LoginDto
            {
                Email = "",
                Password = ""
            };

            _serviceFalso.Setup(s => s.LoginAsync(dto))
                .ReturnsAsync(Result.Failure<LoginResponseDto>("Email y contraseña son obligatorios"));

            // ACTUAR
            var resultado = await _controller.LoginAsync(dto);

            // COMPROBAR
            Assert.That(resultado, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}
