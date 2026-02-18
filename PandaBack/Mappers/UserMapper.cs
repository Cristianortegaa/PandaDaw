using PandaBack.Dtos.Auth;
using PandaBack.Models;

namespace PandaBack.Mappers;

public static class UserMapper
{
    // De Entidad a DTO de Respuesta
    public static UserResponseDto ToDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellidos = user.Apellidos,
            Email = user.Email ?? string.Empty,
            Avatar = user.Avatar,
            Role = user.Role.ToString(), // Convertimos el Enum a String
            FechaAlta = user.FechaAlta
        };
    }

    // De DTO de Registro a Entidad
    public static User ToModel(this RegisterDto dto)
    {
        return new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nombre = dto.Nombre,
            Apellidos = dto.Apellidos,
            Role = Role.User, // Rol por defecto
            FechaAlta = DateTime.UtcNow
        };
    }
}