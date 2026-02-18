using PandaBack.Dtos.Productos;
using PandaBack.Models;

namespace PandaBack.Mappers;

public static class ProductoMapper
{
    public static ProductoResponseDto ToDto(this Producto producto)
    {
        return new ProductoResponseDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Stock = producto.Stock,
            Imagen = producto.Imagen ?? "https://placehold.net/600x400.png", // Imagen por defecto
            Categoria = producto.Category.ToString(), // Enum a String
            IsDeleted = producto.IsDeleted
        };
    }

    public static Producto ToModel(this ProductoRequestDto dto)
    {
        return new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Stock = dto.Stock,
            // Convertir string a Enum (Manejar excepciones en servicio)
            Category = Enum.Parse<Categoria>(dto.Categoria, true),
            IsDeleted = false
        };
    }
}