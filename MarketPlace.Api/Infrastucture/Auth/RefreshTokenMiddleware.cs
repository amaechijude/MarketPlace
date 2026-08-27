using JetBrains.Annotations;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class RefreshTokenMiddleware(
    RequestDelegate next,
    TimeProvider timeProvider,
    IAuthSessionStore authSessionStore,
    ILogger<RefreshTokenMiddleware> logger,
    IWebHostEnvironment env
)
{
    [UsedImplicitly]
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.ExtractToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            context.Response.OnStarting(async () =>
            {
                try
                { // Only refresh if the request was successful
                    if (context.Response.StatusCode is < 200 or >= 300)
                        return;
                    var now = timeProvider.GetUtcNow();
                    var session = await authSessionStore.GetSessionAsync(
                        token,
                        context.RequestAborted
                    );

                    if (session is null || session.ExpiresOn <= now)
                        return;

                    if ((session.ExpiresOn - now).TotalDays >= 2)
                        return;

                    var (accessToken, ttl) = await authSessionStore.RefreshSessionAsync(
                        token,
                        session,
                        context.RequestAborted
                    );

                    context.Response.AttachAccessToken(accessToken, ttl, env);
                }
                catch (Exception ex)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning(ex, "Session refresh failed");
                }
            });
        }

        await next(context);
    }
}
