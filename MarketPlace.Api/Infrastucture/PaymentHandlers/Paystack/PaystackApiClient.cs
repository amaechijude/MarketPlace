using Polly.CircuitBreaker;
using Polly.Timeout;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;

public sealed class PaystackApiClient(HttpClient client, ILogger<PaystackApiClient> logger)
{
    public async Task<PaystackInitResponse?> InitializeTransactionAsync(
        PaystackInitPayload payload,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await client.PostAsJsonAsync(
                "/transaction/initialize",
                payload,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PaystackInitResponse>(
                cancellationToken
            );
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "error while initializing transaction: {Message}", ex.Message);
            return null;
        }
        catch (TimeoutRejectedException ex)
        {
            logger.LogError(
                ex,
                "Request timed out while initializing transaction {message}",
                ex.Message
            );
            return null;
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogError(
                ex,
                "Circuit breaker is open while initializing transaction {message}",
                ex.Message
            );

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize transaction {message}", ex.Message);

            return null;
        }
    }

    /// <summary>
    /// Calls <c>GET /transaction/verify/:reference</c>.
    /// </summary>
    public async Task<VerifyTransactionResponse?> VerifyTransactionAsync(
        string reference,
        CancellationToken cancellationToken
    )
    {
        var url = $"/transaction/verify/{Uri.EscapeDataString(reference)}";
        try
        {
            return await client.GetFromJsonAsync<VerifyTransactionResponse>(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "HTTP error while verifying transaction {refrence} {message}",
                reference,
                ex.Message
            );
            return null;
        }
        catch (TimeoutRejectedException ex)
        {
            logger.LogError(
                ex,
                "Request timed out while verifying transaction {refrence} {message}",
                reference,
                ex.Message
            );
            return null;
        }
        catch (BrokenCircuitException ex)
        {
            logger.LogError(
                ex,
                "Circuit breaker is open while verifying transaction {refrence} {message}",
                reference,
                ex.Message
            );

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to verifying transaction {refrence} {message}",
                reference,
                ex.Message
            );

            return null;
        }
    }
}
