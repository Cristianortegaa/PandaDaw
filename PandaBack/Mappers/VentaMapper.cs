using PandaBack.Dtos.Ventas;
using PandaBack.Models;

namespace PandaBack.Mappers;

/// <summary>
/// Mapper estático para convertir entre Venta/LineaVenta y sus DTOs.
/// </summary>
public static class VentaMapper
{
    /// <summary>
    /// Convierte una Venta a VentaResponseDto.
    /// </summary>
    /// <param name="venta">Venta a convertir.</param>
    /// <returns>DTO de respuesta de venta.</returns>
    public static VentaResponseDto ToDto(this Venta venta)
    {
        return new VentaResponseDto
        {
            Id = venta.Id,
            FechaCompra = venta.FechaCompra,
            Total = venta.Total,
            Estado = venta.Estado.ToString(),
            UsuarioId = venta.UserId.ToString(),
            UsuarioNombre = venta.User != null ? $"{venta.User.Nombre} {venta.User.Apellidos}" : "Usuario Desconocido",
            UsuarioEmail = venta.User?.Email ?? "",
            Lineas = venta.Lineas.Select(l => l.ToDto()).ToList()
        };
    }

    /// <summary>
    /// Convierte una LineaVenta a LineaVentaResponseDto.
    /// </summary>
    /// <param name="linea">Línea de venta a convertir.</param>
    /// <returns>DTO de respuesta de línea de venta.</returns>
    public static LineaVentaResponseDto ToDto(this LineaVenta linea)
    {
        return new LineaVentaResponseDto
        {
            ProductoId = linea.ProductoId,
            ProductoNombre = linea.Producto?.Nombre ?? "Producto no disponible",
            ProductoImagen = linea.Producto?.Imagen ?? "https://via.placeholder.com/50",
            Cantidad = linea.Cantidad,
            PrecioUnitario = linea.PrecioUnitario,
            Subtotal = linea.Subtotal
        };
    }
}