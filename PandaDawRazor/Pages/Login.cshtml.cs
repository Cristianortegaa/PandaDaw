using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Models;

namespace PandaDawRazor.Pages;

public class LoginModel(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        ErrorMessage = null;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var user = await userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ErrorMessage = "Email o contraseña incorrectos";
                return Page();
            }

            var result = await signInManager.PasswordSignInAsync(
                user.UserName!, Input.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Nombre);
                HttpContext.Session.SetString("UserEmail", user.Email!);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());

                logger.LogInformation("Usuario {Email} ha iniciado sesión.", user.Email);
                return LocalRedirect(returnUrl);
            }

            ErrorMessage = "Email o contraseña incorrectos";
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante el inicio de sesión");
            ErrorMessage = "Ocurrió un error al intentar iniciar sesión.";
            return Page();
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email no válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }
}