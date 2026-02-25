using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PandaBack.Data;
using PandaBack.Models;
using PandaBack.Repositories.Auth;
using Testcontainers.PostgreSql;

namespace Tests.Integration
{
    [TestFixture]
    public class AuthRepositoryIntegrationTest
    {
        private static PostgreSqlContainer _postgresContainer = null!;
        private PandaDbContext _context = null!;
        private UserManager<User> _userManager = null!;
        private AuthRepository _repository = null!;
        private IServiceScope _scope = null!;
        private ServiceProvider _serviceProvider = null!;

        [OneTimeSetUp]
        public async Task SetUpContenedorPostgres()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("pandadaw_auth_test")
                .WithUsername("test")
                .WithPassword("test")
                .Build();

            await _postgresContainer.StartAsync();
        }

        [OneTimeTearDown]
        public async Task DestruirContenedorPostgres()
        {
            await _postgresContainer.DisposeAsync();
        }

        [SetUp]
        public void PrepararTodo()
        {
            var services = new ServiceCollection();

            services.AddDbContext<PandaDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            services.AddIdentityCore<User>(options =>
                {
                    // Relajar validaciones para tests
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 4;
                })
                .AddEntityFrameworkStores<PandaDbContext>();

            services.AddLogging();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();

            _context = _scope.ServiceProvider.GetRequiredService<PandaDbContext>();
            _context.Database.EnsureCreated();

            _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            _repository = new AuthRepository(_userManager);
        }

        [TearDown]
        public void LimpiarTodo()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _userManager.Dispose();
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        // ==========================================
        // 1. PRUEBAS DE REGISTRO
        // ==========================================

        [Test]
        public async Task Register_ConDatosCorrectos_DebeCrearUsuario()
        {
            // PREPARAR
            var user = new User
            {
                UserName = "jorge@test.com",
                Email = "jorge@test.com",
                Nombre = "Jorge",
                Apellidos = "Martinez",
                Role = Role.User
            };

            // ACTUAR
            var resultado = await _repository.RegisterAsync(user, "Password123!");

            // COMPROBAR
            Assert.That(resultado.Succeeded, Is.True);

            var usuarioEnBd = await _repository.FindByEmailAsync("jorge@test.com");
            Assert.That(usuarioEnBd, Is.Not.Null);
            Assert.That(usuarioEnBd!.Nombre, Is.EqualTo("Jorge"));
        }

        [Test]
        public async Task Register_ConPasswordDebil_DebeFallar()
        {
            // PREPARAR
            var user = new User
            {
                UserName = "test@test.com",
                Email = "test@test.com",
                Nombre = "Test",
                Apellidos = "User",
                Role = Role.User
            };

            // ACTUAR (contraseña muy corta con nuestra config mínimo 4)
            var resultado = await _repository.RegisterAsync(user, "ab");

            // COMPROBAR
            Assert.That(resultado.Succeeded, Is.False);
            Assert.That(resultado.Errors, Is.Not.Empty);
        }

        [Test]
        public async Task Register_ConEmailDuplicado_DebeFallar()
        {
            // PREPARAR
            var user1 = new User
            {
                UserName = "duplicado@test.com",
                Email = "duplicado@test.com",
                Nombre = "Primero",
                Apellidos = "User",
                Role = Role.User
            };
            await _repository.RegisterAsync(user1, "Password123!");

            var user2 = new User
            {
                UserName = "duplicado@test.com",
                Email = "duplicado@test.com",
                Nombre = "Segundo",
                Apellidos = "User",
                Role = Role.User
            };

            // ACTUAR
            var resultado = await _repository.RegisterAsync(user2, "Password456!");

            // COMPROBAR
            Assert.That(resultado.Succeeded, Is.False);
        }

        // ==========================================
        // 2. PRUEBAS DE BUSCAR POR EMAIL
        // ==========================================

        [Test]
        public async Task FindByEmail_SiExiste_DebeDevolver()
        {
            // PREPARAR
            var user = new User
            {
                UserName = "buscar@test.com",
                Email = "buscar@test.com",
                Nombre = "Buscar",
                Apellidos = "Test",
                Role = Role.User
            };
            await _repository.RegisterAsync(user, "Password123!");

            // ACTUAR
            var resultado = await _repository.FindByEmailAsync("buscar@test.com");

            // COMPROBAR
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado!.Email, Is.EqualTo("buscar@test.com"));
            Assert.That(resultado.Nombre, Is.EqualTo("Buscar"));
        }

        [Test]
        public async Task FindByEmail_SiNoExiste_DebeDevoverNull()
        {
            // ACTUAR
            var resultado = await _repository.FindByEmailAsync("noexiste@test.com");

            // COMPROBAR
            Assert.That(resultado, Is.Null);
        }

        // ==========================================
        // 3. PRUEBAS DE VERIFICAR CONTRASEÑA
        // ==========================================

        [Test]
        public async Task CheckPassword_ConPasswordCorrecta_DebeDevoverTrue()
        {
            // PREPARAR
            var user = new User
            {
                UserName = "check@test.com",
                Email = "check@test.com",
                Nombre = "Check",
                Apellidos = "Password",
                Role = Role.User
            };
            await _repository.RegisterAsync(user, "MiPassword123!");

            var userEnBd = await _repository.FindByEmailAsync("check@test.com");

            // ACTUAR
            var resultado = await _repository.CheckPasswordAsync(userEnBd!, "MiPassword123!");

            // COMPROBAR
            Assert.That(resultado, Is.True);
        }

        [Test]
        public async Task CheckPassword_ConPasswordIncorrecta_DebeDevoverFalse()
        {
            // PREPARAR
            var user = new User
            {
                UserName = "check2@test.com",
                Email = "check2@test.com",
                Nombre = "Check",
                Apellidos = "Password",
                Role = Role.User
            };
            await _repository.RegisterAsync(user, "MiPassword123!");

            var userEnBd = await _repository.FindByEmailAsync("check2@test.com");

            // ACTUAR
            var resultado = await _repository.CheckPasswordAsync(userEnBd!, "ContraseñaMala");

            // COMPROBAR
            Assert.That(resultado, Is.False);
        }
    }
}
