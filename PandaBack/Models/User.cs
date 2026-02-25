using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

/// <summary>
/// Representa un usuario en el sistema.
/// </summary>
[Table("usuarios")]
[Index(nameof(Email), IsUnique = true)]
public class User : IdentityUser
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>
    /// URL del avatar del usuario.
    /// </summary>
    public string? Avatar { get; set; }
    
    /// <summary>
    /// Indica si el usuario ha sido eliminado lógicamente.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Fecha de alta del usuario en el sistema.
    /// </summary>
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización del usuario.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Fecha de eliminación del usuario.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Rol del usuario en el sistema.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(20)")]
    public Role Role { get; set; } = Role.User;
    
    /// <summary>
    /// Carrito activo del usuario.
    /// </summary>
    public virtual Carrito? Carrito { get; set; }
    
    /// <summary>
    /// Colección de ventas realizadas por el usuario.
    /// </summary>
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
