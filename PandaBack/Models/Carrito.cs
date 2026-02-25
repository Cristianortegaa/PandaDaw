﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PandaBack.Models;

/// <summary>
/// Representa un carrito de compras.
/// </summary>
[Table("carts")]
public class Carrito
{
    /// <summary>
    /// Identificador único del carrito.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    /// <summary>
    /// Identificador del usuario propietario del carrito.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Usuario propietario del carrito.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
    
    /// <summary>
    /// Líneas de productos en el carrito.
    /// </summary>
    public virtual ICollection<LineaCarrito> LineasCarrito { get; set; } = new List<LineaCarrito>();
    
    /// <summary>
    /// Fecha de última actualización del carrito.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total del carrito (suma de subtotales).
    /// </summary>
    [NotMapped]
    public decimal Total => LineasCarrito.Sum(linea => linea.Subtotal);

    /// <summary>
    /// Cantidad total de items en el carrito.
    /// </summary>
    [NotMapped]
    public int TotalItems => LineasCarrito.Sum(linea => linea.Cantidad);

    /// <summary>
    /// Agrega una línea al carrito. Si el producto ya existe, suma la cantidad.
    /// </summary>
    /// <param name="item">Línea de carrito a agregar.</param>
    /// <exception cref="InvalidOperationException">Se lanza si no hay stock suficiente.</exception>
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

    /// <summary>
    /// Elimina una línea del carrito.
    /// </summary>
    /// <param name="item">Línea de carrito a eliminar.</param>
    public void RemoveLineaCarrito(LineaCarrito item)
    {
        LineasCarrito.Remove(item);
        item.Carrito = null;
        UpdatedAt = DateTime.UtcNow;
    }
}