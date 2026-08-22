using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Features.Webhooks.Paystack;

public sealed class PaystackWebHookHandler(
    IOptions<PayStackSettings> options,
    IServiceProvider serviceProvider,
    ILogger<PaystackWebHookHandler> logger
) : IRequestHandler
{
    public async Task<bool> HandleWebhookAsync(
        string rawBody,
        string? signature,
        CancellationToken cancellationToken
    )
    {
        if (!IsValidSignature(rawBody, signature))
            return false;

        try
        {
            var payload = JsonSerializer.Deserialize<PaystackWebhookPayload>(rawBody);
            if (payload is null)
            {
                logger.LogWarning("Deserialized webhook payload was null.");
                return true;
            }

            var handler = serviceProvider.GetKeyedService<IPaystackDispatcher>(payload.Event);
            if (handler is null)
                return false;

            await handler.HandleAsync(payload.Data, cancellationToken);

            return true;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize webhook payload.");
            return true; // Signature valid, but payload malformed — still return 200 to Paystack
        }
    }

    private bool IsValidSignature(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return false;

        var secretKeyBytes = Encoding.UTF8.GetBytes(options.Value.SecretKey);

        using var hmacsha512 = new HMACSHA512(secretKeyBytes);

        var payloadbytes = Encoding.UTF8.GetBytes(payload);
        var hashBytes = hmacsha512.ComputeHash(payloadbytes);

        var computedHash = Convert.ToHexString(hashBytes).ToLower();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(signature)
        );
    }
}
