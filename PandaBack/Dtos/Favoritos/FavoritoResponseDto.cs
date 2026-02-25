namespace PandaBack.Dtos.Favoritos;

/// <summary>
/// DTO de respuesta de favorito.
/// </summary>
public class FavoritoResponseDto
{
    /// <summary>
    /// Identificador único del favorito.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Fecha en que se agregó el producto a favoritos.
    /// </summary>
    public DateTime AgregadoEl { get; set; }
    
    /// <summary>
    /// Identificador del producto.
    /// </summary>
    public long ProductoId { get; set; }
    
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    public string ProductoNombre { get; set; } = string.Empty;
    
    /// <summary>
    /// URL de la imagen del producto.
    /// </summary>
    public string ProductoImagen { get; set; } = string.Empty;
    
    /// <summary>
    /// Precio del producto.
    /// </summary>
    public decimal ProductoPrecio { get; set; }
    
    /// <summary>
    /// Categoría del producto.
    /// </summary>
    public string ProductoCategoria { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock disponible del producto.
    /// </summary>
    public int ProductoStock { get; set; }
}