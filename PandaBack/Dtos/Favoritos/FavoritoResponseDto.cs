namespace PandaBack.Dtos.Favoritos;

public class FavoritoResponseDto
{
    public long Id { get; set; } // ID del favorito (para borrarlo)
    public DateTime AgregadoEl { get; set; }
    
    // Datos del Producto aplanados
    public long ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoImagen { get; set; } = string.Empty;
    public decimal ProductoPrecio { get; set; }
    public string ProductoCategoria { get; set; } = string.Empty;
    public int ProductoStock { get; set; }
}