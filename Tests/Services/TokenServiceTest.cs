using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using PandaBack.Models;
using PandaBack.Services.Auth;

namespace Tests.Services
{
    public class TokenServiceTest
    {
        private TokenService _tokenService;
        private const string TestKey = "EstaClaveTieneAlMenos32CaracteresQueEsSuficienteParaHMAC256";
        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";
        private const string TestExpireMinutes = "60";

        [SetUp]
        public void PrepararTodo()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", TestKey },
                { "Jwt:Issuer", TestIssuer },
                { "Jwt:Audience", TestAudience },
                { "Jwt:ExpireInMinutes", TestExpireMinutes }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenService = new TokenService(configuration);
        }

        [Test]
        public void GenerateToken_DebeRetornarTokenNoVacio()
        {
            var user = new User
            {
                Id = "user-123",
                Email = "test@test.com",
                Nombre = "Juan",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);

            Assert.That(token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void GenerateToken_TokenDebeSerJwtValido()
        {
            var user = new User
            {
                Id = "user-123",
                Email = "test@test.com",
                Nombre = "Juan",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();

            Assert.That(handler.CanReadToken(token), Is.True);
        }

        [Test]
        public void GenerateToken_DebeContenerClaimSub()
        {
            var user = new User
            {
                Id = "user-456",
                Email = "test@test.com",
                Nombre = "Test",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            Assert.That(subClaim, Is.Not.Null);
            Assert.That(subClaim!.Value, Is.EqualTo("user-456"));
        }

        [Test]
        public void GenerateToken_DebeContenerClaimEmail()
        {
            var user = new User
            {
                Id = "user-789",
                Email = "juan@test.com",
                Nombre = "Juan",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
            Assert.That(emailClaim, Is.Not.Null);
            Assert.That(emailClaim!.Value, Is.EqualTo("juan@test.com"));
        }

        [Test]
        public void GenerateToken_DebeContenerClaimRole()
        {
            var user = new User
            {
                Id = "admin-id",
                Email = "admin@test.com",
                Nombre = "Admin",
                Role = Role.Admin
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role || c.Type == "role");
            Assert.That(roleClaim, Is.Not.Null);
            Assert.That(roleClaim!.Value, Is.EqualTo("Admin"));
        }

        [Test]
        public void GenerateToken_DebeContenerClaimNombre()
        {
            var user = new User
            {
                Id = "user-id",
                Email = "test@test.com",
                Nombre = "Pedro",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var nameClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Name || c.Type == "unique_name");
            Assert.That(nameClaim, Is.Not.Null);
            Assert.That(nameClaim!.Value, Is.EqualTo("Pedro"));
        }

        [Test]
        public void GenerateToken_DebeContenerClaimJti()
        {
            var user = new User
            {
                Id = "user-id",
                Email = "test@test.com",
                Nombre = "Test",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            Assert.That(jtiClaim, Is.Not.Null);
            Assert.That(jtiClaim!.Value, Is.Not.Empty);
        }

        [Test]
        public void GenerateToken_DosTokens_DebenTenerJtiDiferente()
        {
            var user = new User
            {
                Id = "user-id",
                Email = "test@test.com",
                Nombre = "Test",
                Role = Role.User
            };

            var token1 = _tokenService.GenerateToken(user);
            var token2 = _tokenService.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jti1 = handler.ReadJwtToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            var jti2 = handler.ReadJwtToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            Assert.That(jti1, Is.Not.EqualTo(jti2));
        }

        [Test]
        public void GenerateToken_IssuerDebeSerCorrecto()
        {
            var user = new User
            {
                Id = "user-id",
                Email = "test@test.com",
                Nombre = "Test",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.That(jwtToken.Issuer, Is.EqualTo(TestIssuer));
        }

        [Test]
        public void GenerateToken_AudienceDebeSerCorrecto()
        {
            var user = new User
            {
                Id = "user-id",
                Email = "test@test.com",
                Nombre = "Test",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.That(jwtToken.Audiences.First(), Is.EqualTo(TestAudience));
        }

        [Test]
        public void GenerateToken_ExpiracionDebeSerEnElFuturo()
        {
            var user = new User
            {
                Id = "user-id",
                Email = "test@test.com",
                Nombre = "Test",
                Role = Role.User
            };

            var token = _tokenService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.That(jwtToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
        }
    }
}
