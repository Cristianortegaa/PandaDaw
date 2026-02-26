using CSharpFunctionalExtensions;
using PandaBack.Errors;
using PandaBack.Repositories;
using Stripe; // Asegúrate de tener este using
using Stripe.Checkout;

namespace PandaBack.Services.Stripe;

public class StripeService : IStripeService
{
    private readonly ICarritoRepository _carritoRepository;
    private readonly ILogger<StripeService> _logger;
    private readonly IConfiguration _configuration;

    public StripeService(ICarritoRepository carritoRepository, ILogger<StripeService> logger, IConfiguration configuration)
    {
        _carritoRepository = carritoRepository;
        _logger = logger;
        _configuration = configuration;
        
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<Result<string, PandaError>> CreateCheckoutSessionAsync(string userId, string successUrl, string cancelUrl)
    {
        var carrito = await _carritoRepository.GetByUserIdAsync(userId);
        if (carrito == null || !carrito.LineasCarrito.Any())
            return Result.Failure<string, PandaError>(new CarritoVacioError("El carrito está vacío"));

        // 1. Validar y formatear URLs de retorno
        if (!Uri.IsWellFormedUriString(successUrl, UriKind.Absolute) || !Uri.IsWellFormedUriString(cancelUrl, UriKind.Absolute))
        {
            return Result.Failure<string, PandaError>(new BadRequestError("Las URLs de redirección deben ser absolutas (incluir http/https)"));
        }

        var lineItems = carrito.LineasCarrito.Select(linea => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                // Convertir a long/int para evitar problemas de precisión decimal con Stripe
                UnitAmountDecimal = Math.Round(linea.Producto!.Precio * 100, 0), 
                Currency = "eur",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = linea.Producto.Nombre,
                    Description = linea.Producto.Descripcion ?? "",
                    // 2. SOLO añadir imagen si es una URL absoluta válida
                    Images = ValidarUrlImagen(linea.Producto.Imagen) 
                }
            },
            Quantity = linea.Cantidad
        }).ToList();

        // Los precios de los productos ya incluyen IVA, no se añade línea extra

        // 3. Construir URL de éxito manejando parámetros existentes
        string finalSuccessUrl = successUrl.Contains("?") 
            ? $"{successUrl}&session_id={{CHECKOUT_SESSION_ID}}" 
            : $"{successUrl}?session_id={{CHECKOUT_SESSION_ID}}";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = finalSuccessUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { { "userId", userId } }
        };

        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return Result.Success<string, PandaError>(session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error de Stripe: {Message}", ex.Message);
            return Result.Failure<string, PandaError>(new BadRequestError($"Error de Stripe: {ex.Message}"));
        }
    }

    // Método auxiliar para evitar el error de "Not a valid URL" en imágenes
    private List<string>? ValidarUrlImagen(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        
        // Si la URL no empieza por http, Stripe la rechazará
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("La URL de imagen '{Url}' no es absoluta y será omitida.", url);
            return null;
        }
        
        return new List<string> { url };
    }

    public async Task<Result<string, PandaError>> GetSessionPaymentStatusAsync(string sessionId)
    {
        try
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            return Result.Success<string, PandaError>(session.PaymentStatus);
        }
        catch (StripeException ex)
        {
            return Result.Failure<string, PandaError>(new BadRequestError(ex.Message));
        }
    }
}