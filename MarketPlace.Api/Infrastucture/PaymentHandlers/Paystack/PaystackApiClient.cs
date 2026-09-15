using Polly.CircuitBreaker;
using Polly.Timeout;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;

public sealed class PaystackApiClient(HttpClient client)
{
    public async Task<PaystackInitResponse?> InitializeTransactionAsync(
        PaystackInitPayload payload,
        CancellationToken cancellationToken
    )
    {
        var response = await client.PostAsJsonAsync(
            "/transaction/initialize",
            payload,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaystackInitResponse>(cancellationToken);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<VerifyTransactionResponse> VerifyTransactionAsync(
        string reference,
        CancellationToken cancellationToken
    )
    {
        var url = $"/transaction/verify/{Uri.EscapeDataString(reference)}";

        return (await client.GetFromJsonAsync<VerifyTransactionResponse>(url, cancellationToken))!;
    }
}
