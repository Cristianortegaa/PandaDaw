using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PandaBack.Data;
using PandaBack.Models;
using PandaBack.Repositories;
using PandaBack.Repositories.Auth;
using PandaBack.Services;
using PandaBack.Services.Auth;
using PandaBack.Services.Email;
using PandaBack.Services.Factura;
using PandaBack.Services.Stripe;
using PandaBack.Repository;
using PandaDawRazor.Filters;
using PandaDawRazor.Services;
using QuestPDF.Infrastructure;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde .env (el archivo queda fuera del repo)
PandaBack.config.DotEnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddFolderApplicationModelConvention("/", model =>
    {
        model.Filters.Add(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(NavBadgePageFilter)));
    });
});
builder.Services.AddScoped<NavBadgePageFilter>();

// Session para almacenar UserId
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database
builder.Services.AddDbContext<PandaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PandaDbContext>()
    .AddDefaultTokenProviders();

// Cookie de Identity: rutas de login/acceso denegado
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Repositories
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<ICarritoRepository, CarritoRepository>();
builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IValoracionRepository, ValoracionRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// Services
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IFavoritoService, FavoritoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IValoracionService, ValoracionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PandaBack.Services.Auth.TokenService>();

// Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddScoped<IStripeService, StripeService>();

// QuestPDF (Community license para uso gratuito)
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<IFacturaService, FacturaService>();

// Email (MailKit)
builder.Services.AddScoped<IEmailService, EmailService>();

// Blazor Server + Notificaciones en tiempo real
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<NotificacionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

if (app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();


app.Run();