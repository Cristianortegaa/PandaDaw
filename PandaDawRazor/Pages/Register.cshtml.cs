using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Models;

namespace PandaDawRazor.Pages;

public class RegisterModel(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

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
            var user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                Nombre = Input.Nombre,
                Apellidos = Input.Apellidos,
                Role = Role.User,
                FechaAlta = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Iniciar sesión automáticamente tras registro
                await signInManager.SignInAsync(user, isPersistent: true);

                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Nombre);
                HttpContext.Session.SetString("UserEmail", user.Email!);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());

                logger.LogInformation("Usuario {Email} registrado e iniciado sesión.", user.Email);
                return LocalRedirect(returnUrl);
            }

            ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Error en registro: {Error}", ErrorMessage);
            return Page();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante el registro");
            ErrorMessage = "Ocurrió un error inesperado. Inténtalo de nuevo.";
            return Page();
        }
    }

    public class RegisterInputModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email no válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
