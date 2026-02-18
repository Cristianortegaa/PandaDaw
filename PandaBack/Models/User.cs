using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

[Table("usuarios")]
[Index(nameof(Email), IsUnique = true)] // Email unico en la base de datos (ya configurado en IdentityUser)
public class User : IdentityUser
{
    // Propiedades customizadas del usuario
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    public string? Avatar { get; set; } // Nullable para la foto de perfil
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }

    [Required]
    [Column(TypeName = "varchar(20)")]
    public Role Role { get; set; } = Role.User;
    
    public virtual Carrito? Carrito { get; set; }
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
