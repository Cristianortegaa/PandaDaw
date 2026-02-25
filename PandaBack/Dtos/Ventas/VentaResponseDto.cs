namespace PandaBack.Dtos.Ventas;

/// <summary>
/// DTO de respuesta de venta/pedido.
/// </summary>
public class VentaResponseDto
{
    /// <summary>
    /// Identificador único de la venta.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Fecha de compra de la venta.
    /// </summary>
    public DateTime FechaCompra { get; set; }
    
    /// <summary>
    /// Total de la venta.
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Estado actual del pedido.
    /// </summary>
    public string Estado { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificador del usuario que realizó la compra.
    /// </summary>
    public string UsuarioId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del usuario que realizó la compra.
    /// </summary>
    public string UsuarioNombre { get; set; } = string.Empty; 
    
    /// <summary>
    /// Email del usuario que realizó la compra.
    /// </summary>
    public string UsuarioEmail { get; set; } = string.Empty;

    /// <summary>
    /// Lista de productos comprados.
    /// </summary>
    public List<LineaVentaResponseDto> Lineas { get; set; } = new();
}