namespace PandaBack.Dtos.Auth;

/// <summary>
/// DTO de respuesta con datos del usuario.
/// </summary>
public class UserResponseDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    public string Apellidos { get; set; } = string.Empty;
    
    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del avatar del usuario.
    /// </summary>
    public string? Avatar { get; set; }
    
    /// <summary>
    /// Rol del usuario en el sistema.
    /// </summary>
    public string Role { get; set; } = "User"; 
    
    /// <summary>
    /// Fecha de alta del usuario.
    /// </summary>
    public DateTime FechaAlta { get; set; }
}