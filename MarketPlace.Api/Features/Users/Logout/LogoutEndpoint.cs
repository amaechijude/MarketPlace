using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Features.Users.Logout;

public static class LogoutEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "logout",
                async (
                    IAuthSessionStore authSessionstore,
                    HttpRequest httpRequest,
                    CancellationToken ct
                ) =>
                {
                    var token = httpRequest.ExtractToken();

                    if (string.IsNullOrWhiteSpace(token))
                        return Results.NoContent();

                    await authSessionstore.DeleteAsync(token, ct);

                    return Results.NoContent();
                }
            )
            .RequireAuthorization();
    }
}
