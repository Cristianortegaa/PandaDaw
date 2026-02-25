using System.ComponentModel.DataAnnotations;

namespace PandaBack.Dtos.Auth;

/// <summary>
/// DTO de credentials de inicio de sesión.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}