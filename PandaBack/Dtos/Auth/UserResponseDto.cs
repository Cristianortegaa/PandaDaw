namespace PandaBack.Dtos.Auth;

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = "User"; // Devolvemos el rol como String
    public DateTime FechaAlta { get; set; }
}