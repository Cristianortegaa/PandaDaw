using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Valoraciones;

public class CreateValoracionDto
{
    [Required]
    public long ProductoId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "La valoración debe ser entre 1 y 5 estrellas")]
    public int Estrellas { get; set; }

    [Required]
    [MaxLength(500, ErrorMessage = "La reseña no puede exceder los 500 caracteres")]
    public string Resena { get; set; } = string.Empty;
}