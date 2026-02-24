using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PandaBack.Dtos.Auth;
using PandaBack.Services.Auth;

namespace PandaDawRazor.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginInputModel LoginInput { get; set; } = new();

    [BindProperty]
    public RegisterInputModel RegisterInput { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool ShowRegister { get; set; }

    public void OnGet()
    {
        // Si ya está autenticado, redirigir a inicio
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
        {
            Response.Redirect("/Index");
        }
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var loginDto = new LoginDto
        {
            Email = LoginInput.Email,
            Password = LoginInput.Password
        };

        var result = await _authService.LoginAsync(loginDto);
        
        if (result.IsSuccess)
        {
            // Guardar datos en sesión
            HttpContext.Session.SetString("UserId", result.Value.Id);
            HttpContext.Session.SetString("Token", result.Value.Token);
            HttpContext.Session.SetString("UserName", result.Value.Nombre);
            HttpContext.Session.SetString("UserEmail", result.Value.Email);
            HttpContext.Session.SetString("UserRole", result.Value.Role);
            
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Email o contraseña incorrectos";
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        ShowRegister = true;
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (RegisterInput.Password != RegisterInput.ConfirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden";
            return Page();
        }

        var registerDto = new RegisterDto
        {
            Nombre = RegisterInput.Nombre,
            Apellidos = RegisterInput.Apellidos,
            Email = RegisterInput.Email,
            Password = RegisterInput.Password
        };

        var result = await _authService.RegisterAsync(registerDto);
        
        if (result.IsSuccess)
        {
            SuccessMessage = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
            ShowRegister = false;
            return Page();
        }

        ErrorMessage = "No se pudo crear la cuenta. El email puede estar en uso.";
        return Page();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email no válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
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