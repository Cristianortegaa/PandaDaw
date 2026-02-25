using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Auth;

/// <summary>
/// DTO de datos de registro de usuario.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}