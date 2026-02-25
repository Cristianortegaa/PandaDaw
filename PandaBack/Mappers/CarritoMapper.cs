using PandaBack.Dtos.Carrito;
using PandaBack.Models;

namespace PandaBack.Mappers;

/// <summary>
/// Mapper estático para convertir entre Carrito/LineaCarrito y sus DTOs.
/// </summary>
public static class CarritoMapper
{
    /// <summary>
    /// Convierte un Carrito a CarritoDto.
    /// </summary>
    /// <param name="carrito">Carrito a convertir.</param>
    /// <returns>DTO de respuesta del carrito.</returns>
    public static CarritoDto ToDto(this Carrito carrito)
    {
        return new CarritoDto
        {
            Id = carrito.Id,
            UsuarioId = carrito.UserId.ToString() ?? "",
            Lineas = carrito.LineasCarrito.Select(l => new LineaCarritoDto
            {
                ProductoId = l.ProductoId,
                ProductoNombre = l.Producto?.Nombre ?? "Producto Desconocido",
                ProductoImagen = l.Producto?.Imagen ?? "",
                Cantidad = l.Cantidad,
                PrecioUnitario = l.Producto?.Precio ?? 0,
                Subtotal = l.Subtotal
            }).ToList(),
            Total = carrito.Total,
            TotalItems = carrito.TotalItems
        };
    }
}