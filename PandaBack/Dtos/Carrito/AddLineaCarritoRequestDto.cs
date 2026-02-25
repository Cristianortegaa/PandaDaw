using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Carrito;

/// <summary>
/// DTO de solicitud para agregar una línea al carrito.
/// </summary>
public class AddLineaCarritoRequestDto
{
    /// <summary>
    /// Identificador del producto a agregar.
    /// </summary>
    [Required]
    public long ProductoId { get; set; }

    /// <summary>
    /// Cantidad del producto a agregar.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; }
}

