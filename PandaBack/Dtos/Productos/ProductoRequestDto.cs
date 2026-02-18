using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Productos;

public class ProductoRequestDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal Precio { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [Required]
    public string Categoria { get; set; } = string.Empty; // Recibimos el string del Enum

    // Para subida de ficheros (Opcional en Update)
    public IFormFile? Imagen { get; set; } 
}