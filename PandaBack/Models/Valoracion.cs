using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

[Table("valoraciones")]
[Index(nameof(ProductoId))]
[Index(nameof(UserId), nameof(ProductoId), IsUnique = true)]
public class Valoracion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5 estrellas.")]
    public int Estrellas { get; set; }

    [Required]
    [MaxLength(500, ErrorMessage = "La reseña no puede superar los 500 caracteres.")]
    [Column(TypeName = "text")]
    public string Resena { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public long UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    public long ProductoId { get; set; }

    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
}