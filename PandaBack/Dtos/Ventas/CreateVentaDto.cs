using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Ventas;

/// <summary>
/// DTO de solicitud para crear una venta.
/// </summary>
public class CreateVentaDto
{
    /// <summary>
    /// Dirección de envío del pedido.
    /// </summary>
    [Required]
    public string DireccionEnvio { get; set; } = string.Empty; 
}