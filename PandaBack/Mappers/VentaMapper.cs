using PandaBack.Dtos.Ventas;
using PandaBack.Models;

namespace PandaBack.Mappers;

public static class VentaMapper
{
    public static VentaResponseDto ToDto(this Venta venta)
    {
        return new VentaResponseDto
        {
            Id = venta.Id,
            FechaCompra = venta.FechaCompra,
            Total = venta.Total,
            
            // Convertimos el Enum a String para que sea legible en el JSON
            Estado = venta.Estado.ToString(),
            
            // Mapeo seguro del Usuario (puede ser nulo si no se incluyó en la consulta)
            UsuarioId = venta.UserId.ToString(),
            UsuarioNombre = venta.User != null ? $"{venta.User.Nombre} {venta.User.Apellidos}" : "Usuario Desconocido",
            UsuarioEmail = venta.User?.Email ?? "",

            // Mapeo de la colección de líneas
            Lineas = venta.Lineas.Select(l => l.ToDto()).ToList()
        };
    }

    public static LineaVentaResponseDto ToDto(this LineaVenta linea)
    {
        return new LineaVentaResponseDto
        {
            ProductoId = linea.ProductoId,
            // Accedemos a los datos del Producto si está cargado (Include)
            ProductoNombre = linea.Producto?.Nombre ?? "Producto no disponible",
            ProductoImagen = linea.Producto?.Imagen ?? "https://via.placeholder.com/50",
            
            Cantidad = linea.Cantidad,
            PrecioUnitario = linea.PrecioUnitario,
            Subtotal = linea.Subtotal // Propiedad calculada del modelo
        };
    }
}