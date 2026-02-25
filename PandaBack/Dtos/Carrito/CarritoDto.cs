namespace PandaBack.Dtos.Carrito;

/// <summary>
/// DTO de respuesta del carrito de compras.
/// </summary>
public class CarritoDto
{
    /// <summary>
    /// Identificador único del carrito.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Identificador del usuario propietario del carrito.
    /// </summary>
    public string UsuarioId { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista de líneas de productos en el carrito.
    /// </summary>
    public List<LineaCarritoDto> Lineas { get; set; } = new();
    
    /// <summary>
    /// Total del carrito.
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Cantidad total de items en el carrito.
    /// </summary>
    public int TotalItems { get; set; }
}