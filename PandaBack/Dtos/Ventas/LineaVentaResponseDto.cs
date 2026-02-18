namespace PandaBack.Dtos.Ventas;

public class LineaVentaResponseDto
{
    public long ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoImagen { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}