using CSharpFunctionalExtensions;
using PandaBack.Errors;
using PandaBack.Repositories;
using Stripe.Checkout;

namespace PandaBack.Services.Stripe;

public class StripeService : IStripeService
{
    private readonly ICarritoRepository _carritoRepository;
    private readonly ILogger<StripeService> _logger;

    public StripeService(ICarritoRepository carritoRepository, ILogger<StripeService> logger)
    {
        _carritoRepository = carritoRepository;
        _logger = logger;
    }

    public async Task<Result<string, PandaError>> CreateCheckoutSessionAsync(string userId, string successUrl, string cancelUrl)
    {
        var carrito = await _carritoRepository.GetByUserIdAsync(userId);
        if (carrito == null || !carrito.LineasCarrito.Any())
            return Result.Failure<string, PandaError>(new CarritoVacioError("El carrito está vacío"));

        var lineItems = carrito.LineasCarrito.Select(linea => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmountDecimal = linea.Producto!.Precio * 100, // Stripe usa centimos
                Currency = "eur",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = linea.Producto.Nombre,
                    Description = linea.Producto.Descripcion ?? "",
                    Images = !string.IsNullOrEmpty(linea.Producto.Imagen) 
                        ? new List<string> { linea.Producto.Imagen } 
                        : null
                }
            },
            Quantity = linea.Cantidad
        }).ToList();

        // Añadir línea de IVA (21%)
        var subtotal = carrito.LineasCarrito.Sum(l => (l.Producto?.Precio ?? 0) * l.Cantidad);
        var iva = subtotal * 0.21m;

        lineItems.Add(new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmountDecimal = iva * 100,
                Currency = "eur",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = "IVA (21%)",
                    Description = "Impuesto sobre el Valor Añadido"
                }
            },
            Quantity = 1
        });

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId }
            },
            CustomerEmail = null // Se pedirá en Stripe Checkout
        };

        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(options);
            _logger.LogInformation("Stripe Checkout Session creada: {SessionId} para usuario {UserId}", session.Id, userId);
            return Result.Success<string, PandaError>(session.Url);
        }
        catch (global::Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Error al crear sesión de Stripe Checkout");
            return Result.Failure<string, PandaError>(new BadRequestError($"Error de pago: {ex.Message}"));
        }
    }

    public async Task<Result<string, PandaError>> GetSessionPaymentStatusAsync(string sessionId)
    {
        try
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            return Result.Success<string, PandaError>(session.PaymentStatus);
        }
        catch (global::Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Error al obtener estado de sesión Stripe: {SessionId}", sessionId);
            return Result.Failure<string, PandaError>(new BadRequestError($"Error al verificar pago: {ex.Message}"));
        }
    }
}
