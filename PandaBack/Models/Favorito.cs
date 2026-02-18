using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

[Table("favoritos")]
[Index(nameof(UserId), nameof(ProductoId), IsUnique = true)]
public class Favorito
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public long UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    public long ProductoId { get; set; }

    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
}