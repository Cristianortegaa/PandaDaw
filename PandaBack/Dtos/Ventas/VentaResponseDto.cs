namespace PandaBack.Dtos.Ventas;

public class VentaResponseDto
{
    public long Id { get; set; }
    public DateTime FechaCompra { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty; // Enum convertido a String
    
    // Datos resumen del cliente
    public string UsuarioId { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty; 
    public string UsuarioEmail { get; set; } = string.Empty;

    // Lista de productos comprados
    public List<LineaVentaResponseDto> Lineas { get; set; } = new();
}