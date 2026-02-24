using PandaBack.Models;

namespace Tests.Models
{
    public class UserModelTest
    {
        [Test]
        public void User_PorDefecto_RoleDebeSerUser()
        {
            var user = new User();

            Assert.That(user.Role, Is.EqualTo(Role.User));
        }

        [Test]
        public void User_PorDefecto_IsDeletedDebeSeFalse()
        {
            var user = new User();

            Assert.That(user.IsDeleted, Is.False);
        }

        [Test]
        public void User_PorDefecto_FechaAltaDebeSerUtcNow()
        {
            var antes = DateTime.UtcNow;
            var user = new User();
            var despues = DateTime.UtcNow;

            Assert.That(user.FechaAlta, Is.InRange(antes, despues));
        }

        [Test]
        public void User_PorDefecto_VentasDebeSerColeccionVacia()
        {
            var user = new User();

            Assert.That(user.Ventas, Is.Not.Null);
            Assert.That(user.Ventas.Count, Is.EqualTo(0));
        }

        [Test]
        public void User_PorDefecto_AvatarDebeSerNull()
        {
            var user = new User();

            Assert.That(user.Avatar, Is.Null);
        }

        [Test]
        public void User_PorDefecto_UpdatedAtDebeSerNull()
        {
            var user = new User();

            Assert.That(user.UpdatedAt, Is.Null);
        }

        [Test]
        public void User_PorDefecto_DeletedAtDebeSerNull()
        {
            var user = new User();

            Assert.That(user.DeletedAt, Is.Null);
        }
    }

    public class FavoritoModelTest
    {
        [Test]
        public void Favorito_PorDefecto_CreatedAtDebeSerUtcNow()
        {
            var antes = DateTime.UtcNow;
            var favorito = new Favorito();
            var despues = DateTime.UtcNow;

            Assert.That(favorito.CreatedAt, Is.InRange(antes, despues));
        }
    }

    public class ValoracionModelTest
    {
        [Test]
        public void Valoracion_PorDefecto_CreatedAtDebeSerUtcNow()
        {
            var antes = DateTime.UtcNow;
            var valoracion = new Valoracion();
            var despues = DateTime.UtcNow;

            Assert.That(valoracion.CreatedAt, Is.InRange(antes, despues));
        }
    }
}
