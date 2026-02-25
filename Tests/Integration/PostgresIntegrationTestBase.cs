using Microsoft.EntityFrameworkCore;
using PandaBack.Data;
using Testcontainers.PostgreSql;

namespace Tests.Integration
{
    /// <summary>
    /// Fixture base para tests de integración que necesitan PostgreSQL real.
    /// Usa TestContainers para levantar un contenedor PostgreSQL antes de toda la clase
    /// y lo destruye cuando terminan todos los tests.
    /// </summary>
    [TestFixture]
    public abstract class PostgresIntegrationTestBase
    {
        private static PostgreSqlContainer _postgresContainer = null!;
        protected PandaDbContext Context = null!;

        [OneTimeSetUp]
        public async Task SetUpContenedorPostgres()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("pandadaw_test")
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
        public void PrepararContexto()
        {
            var options = new DbContextOptionsBuilder<PandaDbContext>()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .Options;

            Context = new PandaDbContext(options);
            Context.Database.EnsureCreated();
        }

        [TearDown]
        public void LimpiarContexto()
        {
            // Borrar datos para tener tests aislados pero reusar el contenedor
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
