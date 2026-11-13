namespace MarketPlace.Api.Infrastucture.Email.Sendly;

public sealed class SendlyEmailSender(HttpClient httpClient)
{
    public async Task<bool> SendAsync(
        SendlyEmailRequest data,
        string idempotencyKey,
        CancellationToken ct
    )
    {
        // 1. Create an explicit HttpRequestMessage so we can attach per-request headers safely
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = JsonContent.Create(data),
        };

        // 2. Attach the Idempotency Key header for this specific request only
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        // 3. Await the actual HttpResponseMessage and wrap it in a using statement
        using var response = await httpClient.SendAsync(request, ct);

        // 4. Return true only if the server returned a 2xx success status code
        return response.IsSuccessStatusCode;
    }
}
