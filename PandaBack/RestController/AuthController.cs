using CSharpFunctionalExtensions;
using PandaBack.Dtos.Auth;
using PandaBack.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace PandaBack.RestController;

/// <summary>
/// Proporciona endpoints API para autenticación y registro de usuarios.
/// </summary>
/// <remarks>
/// Este controlador gestiona el registro de nuevos usuarios y el inicio de sesión
/// mediante JWT (JSON Web Tokens).
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="dto">Datos de registro del usuario.</param>
    /// <returns>Los datos del usuario registrado.</returns>
    /// <response code="201">El usuario se registró correctamente.</response>
    /// <response code="400">Si los datos son inválidos o el registro falla.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);

        if (result.IsFailure)
            return BadRequest(new { message = result.Error });

        return Created($"/api/Auth/{result.Value.Id}", result.Value);
    }

    /// <summary>
    /// Inicia sesión y genera un token JWT.
    /// </summary>
    /// <param name="dto">Credenciales de acceso (email y contraseña).</param>
    /// <returns>Token JWT y datos del usuario autenticado.</returns>
    /// <response code="200">Login exitoso. Devuelve el token y datos del usuario.</response>
    /// <response code="401">Credenciales inválidas.</response>
    /// <response code="500">Si ocurre un error interno del servidor.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);

        if (result.IsFailure)
            return Unauthorized(new { message = result.Error });

        return Ok(result.Value);
    }
}
