using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Favoritos;

public class CreateFavoritoDto
{
    [Required]
    public long ProductoId { get; set; }
}