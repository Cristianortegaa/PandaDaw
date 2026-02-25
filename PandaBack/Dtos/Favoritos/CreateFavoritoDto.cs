using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Favoritos;

/// <summary>
/// DTO de solicitud para agregar un producto a favoritos.
/// </summary>
public class CreateFavoritoDto
{
    /// <summary>
    /// Identificador del producto a agregar a favoritos.
    /// </summary>
    [Required]
    public long ProductoId { get; set; }
}