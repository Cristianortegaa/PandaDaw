using CSharpFunctionalExtensions;
using PandaBack.Errors;

namespace PandaBack.Services.Stripe;

public interface IStripeService
{
    Task<Result<string, PandaError>> CreateCheckoutSessionAsync(string userId, string successUrl, string cancelUrl);
    Task<Result<string, PandaError>> GetSessionPaymentStatusAsync(string sessionId);
}
