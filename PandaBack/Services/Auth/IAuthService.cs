using CSharpFunctionalExtensions;
using PandaBack.Dtos.Auth;

namespace PandaBack.Services.Auth;

public interface IAuthService
{
    Task<Result<UserResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto loginDto);
}