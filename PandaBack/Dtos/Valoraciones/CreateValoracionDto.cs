using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Valoraciones;

/// <summary>
/// DTO de solicitud para crear una valoración.
/// </summary>
public class CreateValoracionDto
{
    /// <summary>
    /// Identificador del producto a valorar.
    /// </summary>
    [Required]
    public long ProductoId { get; set; }

    /// <summary>
    /// Número de estrellas (1-5).
    /// </summary>
    [Required]
    [Range(1, 5, ErrorMessage = "La valoración debe ser entre 1 y 5 estrellas")]
    public int Estrellas { get; set; }

    /// <summary>
    /// Texto de la reseña.
    /// </summary>
    [Required]
    [MaxLength(500, ErrorMessage = "La reseña no puede exceder los 500 caracteres")]
    public string Resena { get; set; } = string.Empty;
}