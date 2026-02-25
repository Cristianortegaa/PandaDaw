using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PandaBack.Models;

/// <summary>
/// Representa un producto en el sistema.
/// </summary>
[Table("productos")]
[Index(nameof(Category))]
[Index(nameof(IsDeleted))]
public class Producto
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción detallada del producto.
    /// </summary>
    [Column(TypeName = "text")] 
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio del producto.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    /// <summary>
    /// URL de la imagen del producto.
    /// </summary>
    public string? Imagen { get; set; }

    /// <summary>
    /// Cantidad en stock del producto.
    /// </summary>
    [Required]
    public int Stock { get; set; }
    
    /// <summary>
    /// Categoría del producto.
    /// </summary>
    [Required]
    [Column(TypeName = "varchar(50)")]
    public Categoria Category { get; set; }
    
    /// <summary>
    /// Fecha de alta del producto en el sistema.
    /// </summary>
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica si el producto ha sido eliminado lógicamente.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Verifica si hay stock disponible para una cantidad específica.
    /// </summary>
    /// <param name="quantity">Cantidad a verificar.</param>
    /// <returns>True si hay stock suficiente, false en caso contrario.</returns>
    public bool HasStock(int quantity)
    {
        return !IsDeleted && Stock >= quantity;
    }

    /// <summary>
    /// Reduce el stock del producto.
    /// </summary>
    /// <param name="quantity">Cantidad a reducir.</param>
    /// <exception cref="InvalidOperationException">Se lanza si no hay stock suficiente.</exception>
    public void ReduceStock(int quantity)
    {
        if (!HasStock(quantity))
        {
            throw new InvalidOperationException($"Stock insuficiente para el producto: {Nombre}");
        }
        Stock -= quantity;
    }

    /// <summary>
    /// Incrementa el stock del producto.
    /// </summary>
    /// <param name="quantity">Cantidad a incrementar.</param>
    public void IncreaseStock(int quantity)
    {
        Stock += quantity;
    }
}