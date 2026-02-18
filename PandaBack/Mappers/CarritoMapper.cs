using PandaBack.Dtos.Carrito;
using PandaBack.Models;

namespace PandaBack.Mappers;

public static class CarritoMapper
{
    public static CarritoDto ToDto(this Carrito carrito)
    {
        return new CarritoDto
        {
            Id = carrito.Id,
            UsuarioId = carrito.UserId.ToString() ?? "",
            // Mapeamos las líneas usando una función lambda
            Lineas = carrito.LineasCarrito.Select(l => new LineaCarritoDto
            {
                ProductoId = l.ProductoId,
                ProductoNombre = l.Producto?.Nombre ?? "Producto Desconocido",
                ProductoImagen = l.Producto?.Imagen ?? "",
                Cantidad = l.Cantidad,
                PrecioUnitario = l.Producto?.Precio ?? 0,
                Subtotal = l.Subtotal // Propiedad calculada del modelo
            }).ToList(),
            Total = carrito.Total, // Propiedad calculada del modelo
            TotalItems = carrito.TotalItems
        };
    }
}