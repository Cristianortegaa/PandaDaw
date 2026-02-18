namespace PandaBack.Dtos.Productos;

public class ProductoResponseDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Imagen { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}