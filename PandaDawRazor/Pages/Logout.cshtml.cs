using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Models;

namespace PandaDawRazor.Pages;

public class LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        logger.LogInformation("Usuario ha cerrado sesión.");
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        logger.LogInformation("Usuario ha cerrado sesión.");
        return RedirectToPage("/Index");
    }
}
