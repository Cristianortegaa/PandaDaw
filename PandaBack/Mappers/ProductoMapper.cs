using PandaBack.Dtos.Productos;
using PandaBack.Models;

namespace PandaBack.Mappers;

/// <summary>
/// Mapper estático para convertir entre Producto y sus DTOs.
/// </summary>
public static class ProductoMapper
{
    /// <summary>
    /// Convierte un Producto a ProductoResponseDto.
    /// </summary>
    /// <param name="producto">Producto a convertir.</param>
    /// <returns>DTO de respuesta de producto.</returns>
    public static ProductoResponseDto ToDto(this Producto producto)
    {
        return new ProductoResponseDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Stock = producto.Stock,
            Imagen = producto.Imagen ?? "https://placehold.net/600x400.png", 
            Categoria = producto.Category.ToString(), 
            IsDeleted = producto.IsDeleted
        };
    }

    /// <summary>
    /// Convierte un ProductoRequestDto a Producto.
    /// </summary>
    /// <param name="dto">DTO de solicitud de producto.</param>
    /// <returns>Producto.</returns>
    public static Producto ToModel(this ProductoRequestDto dto)
    {
        return new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Stock = dto.Stock,
            Category = Enum.Parse<Categoria>(dto.Categoria, true),
            IsDeleted = false
        };
    }
}