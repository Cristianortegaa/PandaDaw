namespace PandaBack.Dtos.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; 
    public string? Avatar { get; set; } 
}