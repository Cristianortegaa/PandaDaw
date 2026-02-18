using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

[Table("cart_items")]
public class LineaCarrito
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; } = 1;
    
    public long CarritoId { get; set; }

    [ForeignKey("CarritoId")]
    public virtual Carrito? Carrito { get; set; }
    
    public long ProductoId { get; set; }

    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
    
    [NotMapped]
    public decimal Subtotal => (Producto?.Precio ?? 0) * Cantidad;

    public void IncrementQuantity(int amount)
    {
        Cantidad += amount;
    }
}