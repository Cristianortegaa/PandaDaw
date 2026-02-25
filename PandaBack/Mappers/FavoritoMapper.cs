using PandaBack.Dtos.Favoritos;
using PandaBack.Models;

namespace PandaBack.Mappers;

/// <summary>
/// Mapper estático para convertir entre Favorito y su DTO.
/// </summary>
public static class FavoritoMapper
{
    /// <summary>
    /// Convierte un Favorito a FavoritoResponseDto.
    /// </summary>
    /// <param name="favorito">Favorito a convertir.</param>
    /// <returns>DTO de respuesta del favorito.</returns>
    public static FavoritoResponseDto ToDto(this Favorito favorito)
    {
        return new FavoritoResponseDto
        {
            Id = favorito.Id,
            AgregadoEl = favorito.CreatedAt,
            ProductoId = favorito.ProductoId,
            ProductoNombre = favorito.Producto?.Nombre ?? "Producto no disponible",
            ProductoImagen = favorito.Producto?.Imagen ?? "https://via.placeholder.com/150",
            ProductoPrecio = favorito.Producto?.Precio ?? 0,
            ProductoStock = favorito.Producto?.Stock ?? 0,
            ProductoCategoria = favorito.Producto?.Category.ToString() ?? "Sin Categoría"
        };
    }
}