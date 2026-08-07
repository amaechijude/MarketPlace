using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Users.Register;

public static class RegisterEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(
            "register",
            async (
                [FromBody] RegisterUserRequest request,
                [FromServices] RegisterUserHandler handler,
                CancellationToken ct
            ) => Results.Ok(await handler.CreateAsync(request, ct))
        );
    }
}
