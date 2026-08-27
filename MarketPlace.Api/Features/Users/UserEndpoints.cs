using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.ForgotPassword;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.RateLimiting;

namespace MarketPlace.Api.Features.Users;

public sealed class UserEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("users").WithTags("Users");

        RegisterEndpoint.Map(group);
        LoginEnpoint.Map(group);
        ForgotPasswordEndpoint.Map(group);

        group.MapPost("logout", HandleLogout).RequireAuthorization();

        group
            .MapGet(
                "/",
                (HttpContext httpContext) =>
                    Results.Ok(
                        new
                        {
                            Message = "Helo",
                            Datetime = DateTime.UtcNow.ToLocalTime(),
                            IpAddress = httpContext.GetClientIp(),
                            Directory = Directory.GetCurrentDirectory(),
                        }
                    )
            )
            .RequireRateLimiting(RateLimitPolicyKeys.LoginTokenBucket);
    }

    private static async ValueTask<IResult> HandleLogout(
        IAuthSessionStore authSessionstore,
        HttpRequest httpRequest,
        CancellationToken ct
    )
    {
        var token = httpRequest.ExtractToken();

        if (string.IsNullOrWhiteSpace(token))
            return Results.NoContent();

        await authSessionstore.DeleteAsync(token, ct);

        return Results.NoContent();
    }
}
