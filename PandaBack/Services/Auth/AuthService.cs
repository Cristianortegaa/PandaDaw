using CSharpFunctionalExtensions;
using PandaBack.Dtos.Auth;
using PandaBack.Mappers;
using PandaBack.Repositories.Auth;

namespace PandaBack.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly TokenService _tokenService; 
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository authRepository, TokenService tokenService, ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<UserResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var user = dto.ToModel();
        var result = await _authRepository.RegisterAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<UserResponseDto>(errorMsg);
        }

        _logger.LogInformation($"Usuario {user.Email} registrado correctamente.");
        return Result.Success(user.ToDto());
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _authRepository.FindByEmailAsync(dto.Email);
    
        if (user is null || !await _authRepository.CheckPasswordAsync(user, dto.Password))
        {
            return Result.Failure<LoginResponseDto>("Credenciales inválidas");
        }

        var token = _tokenService.GenerateToken(user);
    
        var response = new LoginResponseDto 
        { 
            Token = token,
            Id = user.Id,
            Nombre = user.Nombre,
            Apellidos = user.Apellidos,
            Email = user.Email!,
            Role = user.Role.ToString(),
            Avatar = user.Avatar ?? "https://via.placeholder.com/150"
        };

        return Result.Success(response);
    }
}