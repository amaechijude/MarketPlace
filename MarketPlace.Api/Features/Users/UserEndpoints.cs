using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Features.Users;

public sealed class UserEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("users").WithTags("Users");

        RegisterEndpoint.Map(group);
        LoginEnpoint.Map(group);

        group.MapGet("/", () => "Hello").RequireAuthorization();
        group.MapGet("/authz", () => "Hello").RequireAuthorization(p => p.RequireRole("role"));

        group.MapPost(
            "logout",
            async (AuthSessionstore authSessionstore, HttpRequest request, CancellationToken ct) =>
            {
                var token = AuthSessionOptions.ExtractToken(request);

                if (string.IsNullOrWhiteSpace(token))
                    return Results.NoContent();

                await authSessionstore.DeleteAsync(token, ct);

                return Results.NoContent();
            }
        );
    }
}
