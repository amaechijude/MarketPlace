using JetBrains.Annotations;
using MarketPlace.Api.Common.Extensions;

namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

public sealed class RefreshTokenMiddleware(
    RequestDelegate next,
    TimeProvider timeProvider,
    IAuthSessionStore authSessionStore,
    ILogger<RefreshTokenMiddleware> logger,
    IHostEnvironment env
)
{
    [UsedImplicitly]
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(async () =>
        {
            try
            { // Only refresh if the request was successful
                if (context.Response.StatusCode is < 200 or >= 300)
                    return;

                // extract token
                var token = context.Request.ExtractToken();
                if (string.IsNullOrWhiteSpace(token))
                    return;

                var now = timeProvider.GetUtcNow();
                var session = await authSessionStore.GetSessionAsync(token, context.RequestAborted);

                if (
                    session is null
                    || session.ExpiresOn <= now
                    || (session.ExpiresOn - now).TotalDays >= 2
                )
                    return;

                var (accessToken, expiresOn) = await authSessionStore.RefreshSessionAsync(
                    token,
                    session,
                    context.RequestAborted
                );

                context.Response.AttachAccessToken(accessToken, expiresOn, env);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning(ex, "Session refresh failed");
            }
        });
        // }

        await next(context);
    }
}
