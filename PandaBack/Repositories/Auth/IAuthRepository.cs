using Microsoft.AspNetCore.Identity;
using PandaBack.Models;

namespace PandaBack.Repositories.Auth;

public interface IAuthRepository
{
    Task<IdentityResult> RegisterAsync(User user, string password);
    Task<User?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
}