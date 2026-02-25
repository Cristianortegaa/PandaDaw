using CSharpFunctionalExtensions;
using PandaBack.Dtos.Auth;

namespace PandaBack.Services.Auth;

/// <summary>
/// Interfaz que define las operaciones del servicio de autenticación.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="registerDto">Datos de registro del usuario.</param>
    /// <returns>Resultado con los datos del usuario registrado o error.</returns>
    Task<Result<UserResponseDto>> RegisterAsync(RegisterDto registerDto);
    
    /// <summary>
    /// Inicia sesión con credenciales de usuario.
    /// </summary>
    /// <param name="loginDto">Credenciales de acceso.</param>
    /// <returns>Resultado con el token de acceso o error.</returns>
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto);
}