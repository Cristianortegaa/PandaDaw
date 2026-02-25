namespace PandaBack.Dtos.Ventas;

/// <summary>
/// DTO de respuesta de una línea de venta.
/// </summary>
public class LineaVentaResponseDto
{
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
    /// Cantidad comprada del producto.
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    public decimal PrecioUnitario { get; set; }
    
    /// <summary>
    /// Subtotal (cantidad * precio unitario).
    /// </summary>
    public decimal Subtotal { get; set; }
}