using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

/// <summary>
/// Representa una línea de producto dentro de una venta.
/// </summary>
[Table("lineas_venta")]
public class LineaVenta
{
    /// <summary>
    /// Identificador único de la línea de venta.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Cantidad de productos comprados.
    /// </summary>
    [Required]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del producto en el momento de la compra.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Subtotal (cantidad * precio unitario).
    /// </summary>
    [NotMapped] 
    public decimal Subtotal => Cantidad * PrecioUnitario;
    
    /// <summary>
    /// Identificador de la venta a la que pertenece esta línea.
    /// </summary>
    public long VentaId { get; set; }

    /// <summary>
    /// Venta a la que pertenece esta línea.
    /// </summary>
    [ForeignKey("VentaId")]
    public virtual Venta? Venta { get; set; }


    /// <summary>
    /// Identificador del producto.
    /// </summary>
    public long ProductoId { get; set; }

    /// <summary>
    /// Producto asociado a esta línea de venta.
    /// </summary>
    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
}