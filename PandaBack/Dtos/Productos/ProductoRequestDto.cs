using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Productos;

/// <summary>
/// DTO de solicitud de producto.
/// </summary>
public class ProductoRequestDto
{
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Precio del producto.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Precio { get; set; }
    
    /// <summary>
    /// Cantidad en stock del producto.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    /// <summary>
    /// Categoría del producto.
    /// </summary>
    [Required]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Archivo de imagen del producto.
    /// </summary>
    public IFormFile? Imagen { get; set; } 
}