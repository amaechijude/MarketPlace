namespace MarketPlace.Api.Features.Webhooks.Paystack;

public static class PaystackEndpoint
{
    public static void Map(RouteGroupBuilder app)
    {
        app.MapPost(
                "paystack",
                async (
                    HttpContext httpContext,
                    PaystackWebHookHandler handler,
                    CancellationToken ct
                ) =>
                {
                    httpContext.Request.EnableBuffering();

                    using var reader = new StreamReader(httpContext.Request.Body);
                    var rawBody = await reader.ReadToEndAsync(ct);

                    httpContext.Request.Headers.TryGetValue(
                        "x-paystack-signature",
                        out var signature
                    );

                    if (string.IsNullOrEmpty(signature))
                        return Results.Ok();

                    _ = await handler.HandleWebhookAsync(rawBody, signature, ct);

                    return Results.Ok();
                }
            )
            .AllowAnonymous()
            .ExcludeFromDescription();
    }
}
