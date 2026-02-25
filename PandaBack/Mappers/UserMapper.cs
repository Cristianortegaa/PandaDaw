using PandaBack.Dtos.Auth;
using PandaBack.Models;

namespace PandaBack.Mappers;

/// <summary>
/// Mapper estático para convertir entre User y sus DTOs.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Convierte un User a UserResponseDto.
    /// </summary>
    /// <param name="user">Usuario a convertir.</param>
    /// <returns>DTO de respuesta del usuario.</returns>
    public static UserResponseDto ToDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellidos = user.Apellidos,
            Email = user.Email ?? string.Empty,
            Avatar = user.Avatar,
            Role = user.Role.ToString(),
            FechaAlta = user.FechaAlta
        };
    }

    /// <summary>
    /// Convierte un RegisterDto a User.
    /// </summary>
    /// <param name="dto">DTO de registro.</param>
    /// <returns>Usuario.</returns>
    public static User ToModel(this RegisterDto dto)
    {
        return new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nombre = dto.Nombre,
            Apellidos = dto.Apellidos,
            Role = Role.User,
            FechaAlta = DateTime.UtcNow
        };
    }
}