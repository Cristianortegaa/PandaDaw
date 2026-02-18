using PandaBack.Dtos.Valoraciones;
using PandaBack.Models;

namespace PandaBack.Mappers;

public static class ValoracionMapper
{
    public static ValoracionResponseDto ToDto(this Valoracion valoracion)
    {
        return new ValoracionResponseDto
        {
            Id = valoracion.Id,
            Estrellas = valoracion.Estrellas,
            Resena = valoracion.Resena,
            Fecha = valoracion.CreatedAt,
            
            UsuarioId = valoracion.UserId.ToString(),
            UsuarioNombre = valoracion.User != null 
                ? $"{valoracion.User.Nombre} {valoracion.User.Apellidos}" 
                : "Usuario Anónimo",
            UsuarioAvatar = valoracion.User?.Avatar ?? "",
            
            // Datos del Producto (útil si listamos "Mis Reseñas")
            ProductoId = valoracion.ProductoId,
            ProductoNombre = valoracion.Producto?.Nombre ?? ""
        };
    }

    public static Valoracion ToModel(this CreateValoracionDto dto, long userId)
    {
        return new Valoracion
        {
            ProductoId = dto.ProductoId,
            UserId = userId, // Se pasa aparte porque viene del Token
            Estrellas = dto.Estrellas,
            Resena = dto.Resena,
            CreatedAt = DateTime.UtcNow
        };
    }
}