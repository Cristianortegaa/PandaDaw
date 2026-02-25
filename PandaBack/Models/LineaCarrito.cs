using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

/// <summary>
/// Representa una línea de producto dentro de un carrito de compras.
/// </summary>
[Table("cart_items")]
public class LineaCarrito
{
    /// <summary>
    /// Identificador único de la línea de carrito.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Cantidad del producto en el carrito.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; } = 1;
    
    /// <summary>
    /// Identificador del carrito al que pertenece esta línea.
    /// </summary>
    public long CarritoId { get; set; }

    /// <summary>
    /// Carrito al que pertenece esta línea.
    /// </summary>
    [ForeignKey("CarritoId")]
    public virtual Carrito? Carrito { get; set; }
    
    /// <summary>
    /// Identificador del producto.
    /// </summary>
    public long ProductoId { get; set; }

    /// <summary>
    /// Producto asociado a esta línea de carrito.
    /// </summary>
    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
    
    /// <summary>
    /// Subtotal (precio del producto * cantidad).
    /// </summary>
    [NotMapped]
    public decimal Subtotal => (Producto?.Precio ?? 0) * Cantidad;

    /// <summary>
    /// Incrementa la cantidad del producto en el carrito.
    /// </summary>
    /// <param name="amount">Cantidad a incrementar.</param>
    public void IncrementQuantity(int amount)
    {
        Cantidad += amount;
    }
}