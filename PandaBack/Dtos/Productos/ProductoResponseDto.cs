namespace PandaBack.Dtos.Productos;

/// <summary>
/// DTO de respuesta de producto.
/// </summary>
public class ProductoResponseDto
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Precio del producto.
    /// </summary>
    public decimal Precio { get; set; }
    
    /// <summary>
    /// Cantidad en stock del producto.
    /// </summary>
    public int Stock { get; set; }
    
    /// <summary>
    /// URL de la imagen del producto.
    /// </summary>
    public string Imagen { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoría del producto.
    /// </summary>
    public string Categoria { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el producto está eliminado lógicamente.
    /// </summary>
    public bool IsDeleted { get; set; }
}