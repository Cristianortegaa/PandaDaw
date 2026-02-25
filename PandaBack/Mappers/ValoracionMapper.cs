﻿using PandaBack.Dtos.Valoraciones;
using PandaBack.Models;

namespace PandaBack.Mappers;

/// <summary>
/// Mapper estático para convertir entre Valoracion y sus DTOs.
/// </summary>
public static class ValoracionMapper
{
    /// <summary>
    /// Convierte una Valoracion a ValoracionResponseDto.
    /// </summary>
    /// <param name="valoracion">Valoracion a convertir.</param>
    /// <returns>DTO de respuesta de valoracion.</returns>
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
            ProductoId = valoracion.ProductoId,
            ProductoNombre = valoracion.Producto?.Nombre ?? ""
        };
    }

    /// <summary>
    /// Convierte un CreateValoracionDto a Valoracion.
    /// </summary>
    /// <param name="dto">DTO de creación de valoracion.</param>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>Valoracion.</returns>
    public static Valoracion ToModel(this CreateValoracionDto dto, string userId)
    {
        return new Valoracion
        {
            ProductoId = dto.ProductoId,
            UserId = userId,
            Estrellas = dto.Estrellas,
            Resena = dto.Resena,
            CreatedAt = DateTime.UtcNow
        };
    }
}