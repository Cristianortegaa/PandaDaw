using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

[Table("ventas")]
public class Venta
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")] 
    public decimal Total { get; set; }

    [Required]
    [Column(TypeName = "varchar(50)")]
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    
    public long UserId { get; set; } 

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    public virtual ICollection<LineaVenta> Lineas { get; set; } = new List<LineaVenta>();

    public void AddLineaVenta(LineaVenta linea)
    {
        Lineas.Add(linea);
        linea.Venta = this; 
        CalculateTotal(); 
    }

    public void CalculateTotal()
    {
        Total = Lineas.Sum(l => l.Subtotal);
    }

    // No se puede modificar pedidos cancelados o entregados
    public void UpdateEstado(EstadoPedido nuevoEstado)
    {
        if (Estado == EstadoPedido.Cancelado)
            throw new InvalidOperationException("No se puede modificar un pedido cancelado");
        
        if (Estado == EstadoPedido.Entregado && nuevoEstado != EstadoPedido.Entregado)
            throw new InvalidOperationException("No se puede modificar un pedido entregado");
        
        Estado = nuevoEstado;
    }
}