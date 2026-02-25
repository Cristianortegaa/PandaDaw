using Microsoft.AspNetCore.Identity;
using PandaBack.Models;

namespace PandaBack.Repositories.Auth;

/// <summary>
/// Interfaz que define las operaciones de acceso a datos de autenticación.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="user">Usuario a registrar.</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <returns>Resultado de la operación de registro.</returns>
    Task<IdentityResult> RegisterAsync(User user, string password);
    
    /// <summary>
    /// Busca un usuario por su correo electrónico.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <returns>Usuario encontrado o null.</returns>
    Task<User?> FindByEmailAsync(string email);
    
    /// <summary>
    /// Verifica la contraseña de un usuario.
    /// </summary>
    /// <param name="user">Usuario a verificar.</param>
    /// <param name="password">Contraseña a verificar.</param>
    /// <returns>True si la contraseña es correcta, false en caso contrario.</returns>
    Task<bool> CheckPasswordAsync(User user, string password);
}