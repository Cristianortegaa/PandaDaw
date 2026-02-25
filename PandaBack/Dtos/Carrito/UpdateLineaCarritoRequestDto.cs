using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Carrito;

/// <summary>
/// DTO de solicitud para actualizar la cantidad de una línea del carrito.
/// </summary>
public class UpdateLineaCarritoRequestDto
{
    /// <summary>
    /// Nueva cantidad del producto.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; }
}

