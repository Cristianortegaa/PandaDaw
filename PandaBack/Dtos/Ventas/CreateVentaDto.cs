using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Ventas;

public class CreateVentaDto
{
    [Required]
    public string DireccionEnvio { get; set; } = string.Empty; 
    // El resto se saca del Carrito del usuario autenticado
}