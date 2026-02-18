namespace PandaBack.Dtos.Carrito;

public class CarritoDto
{
    public long Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public List<LineaCarritoDto> Lineas { get; set; } = new();
    public decimal Total { get; set; }
    public int TotalItems { get; set; }
}