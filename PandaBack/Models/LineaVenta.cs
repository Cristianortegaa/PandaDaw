using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

[Table("lineas_venta")]
public class LineaVenta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public int Cantidad { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [NotMapped] 
    public decimal Subtotal => Cantidad * PrecioUnitario;
    
    public long VentaId { get; set; }

    [ForeignKey("VentaId")]
    public virtual Venta? Venta { get; set; }


    public long ProductoId { get; set; }

    [ForeignKey("ProductoId")]
    public virtual Producto? Producto { get; set; }
}