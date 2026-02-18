using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

[Table("carts")]
public class Carrito
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    public long UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    public virtual ICollection<LineaCarrito> LineasCarrito { get; set; } = new List<LineaCarrito>();
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public decimal Total => LineasCarrito.Sum(linea => linea.Subtotal);

    [NotMapped]
    public int TotalItems => LineasCarrito.Sum(linea => linea.Cantidad);

    // Si el producto ya existe, suma la cantidad. Si no, añade nueva línea
    public void AddLineaCarrito(LineaCarrito item)
    {
        LineasCarrito ??= new List<LineaCarrito>();
        
        var lineaExistente = LineasCarrito.FirstOrDefault(l => l.ProductoId == item.ProductoId);

        if (lineaExistente != null)
        {
            if (item.Producto?.HasStock(lineaExistente.Cantidad + item.Cantidad) == false)
                throw new InvalidOperationException("Stock insuficiente");
                
            lineaExistente.IncrementQuantity(item.Cantidad);
        }
        else
        {
            if (item.Producto?.HasStock(item.Cantidad) == false)
                throw new InvalidOperationException("Stock insuficiente");
                
            LineasCarrito.Add(item);
            item.Carrito = this;
        }
            
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLineaCarrito(LineaCarrito item)
    {
        LineasCarrito.Remove(item);
        item.Carrito = null;
        UpdatedAt = DateTime.UtcNow;
    }
}