using PandaBack.Dtos.Favoritos;
using PandaBack.Models;

namespace PandaBack.Mappers;

public static class FavoritoMapper
{
    public static FavoritoResponseDto ToDto(this Favorito favorito)
    {
        return new FavoritoResponseDto
        {
            Id = favorito.Id,
            AgregadoEl = favorito.CreatedAt,
            
            // Aplanamos el objeto Producto
            // Nota: Requiere usar .Include(f => f.Producto) en la consulta
            ProductoId = favorito.ProductoId,
            ProductoNombre = favorito.Producto?.Nombre ?? "Producto no disponible",
            ProductoImagen = favorito.Producto?.Imagen ?? "https://via.placeholder.com/150",
            ProductoPrecio = favorito.Producto?.Precio ?? 0,
            ProductoStock = favorito.Producto?.Stock ?? 0,
            ProductoCategoria = favorito.Producto?.Category.ToString() ?? "Sin Categoría"
        };
    }
}