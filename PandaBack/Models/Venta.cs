﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

/// <summary>
/// Representa una venta (pedido) en el sistema.
/// </summary>
[Table("ventas")]
public class Venta
{
    /// <summary>
    /// Identificador único de la venta.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// Fecha en que se realizó la compra.
    /// </summary>
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total de la venta.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")] 
    public decimal Total { get; set; }

    /// <summary>
    /// Estado actual del pedido.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(50)")]
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    
    /// <summary>
    /// Identificador del usuario que realizó la compra.
    /// </summary>
    public string UserId { get; set; } = string.Empty; 

    /// <summary>
    /// Usuario que realizó la compra.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Líneas de productos de la venta.
    /// </summary>
    public virtual ICollection<LineaVenta> Lineas { get; set; } = new List<LineaVenta>();

    /// <summary>
    /// Agrega una línea de venta y recalcula el total.
    /// </summary>
    /// <param name="linea">Línea de venta a agregar.</param>
    public void AddLineaVenta(LineaVenta linea)
    {
        Lineas.Add(linea);
        linea.Venta = this; 
        CalculateTotal(); 
    }

    /// <summary>
    /// Calcula el total de la venta sumando los subtotales de las líneas.
    /// </summary>
    public void CalculateTotal()
    {
        Total = Lineas.Sum(l => l.Subtotal);
    }

    /// <summary>
    /// Actualiza el estado del pedido.
    /// </summary>
    /// <param name="nuevoEstado">Nuevo estado del pedido.</param>
    /// <exception cref="InvalidOperationException">Se lanza si el pedido está cancelado o entregado.</exception>
    public void UpdateEstado(EstadoPedido nuevoEstado)
    {
        if (Estado == EstadoPedido.Cancelado)
            throw new InvalidOperationException("No se puede modificar un pedido cancelado");
        
        if (Estado == EstadoPedido.Entregado && nuevoEstado != EstadoPedido.Entregado)
            throw new InvalidOperationException("No se puede modificar un pedido entregado");
        
        Estado = nuevoEstado;
    }
}