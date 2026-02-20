using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Carrito;

public class AddLineaCarritoRequestDto
{
    [Required]
    public long ProductoId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; }
}

