namespace PandaBack.Dtos.Auth;

/// <summary>
/// DTO de respuesta de inicio de sesión.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Token JWT de autenticación.
    /// </summary>
    public string Token { get; set; } = string.Empty;

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
    /// Rol del usuario en el sistema.
    /// </summary>
    public string Role { get; set; } = string.Empty; 
    
    /// <summary>
    /// URL del avatar del usuario.
    /// </summary>
    public string? Avatar { get; set; } 
}