using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Users.Register;

public static class RegisterEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group = group.MapGroup("register");

        group
            .MapPost(
                "/",
                async (
                    [FromBody] RegisterUserRequest request,
                    [FromServices] RegisterUserHandler handler,
                    CancellationToken ct
                ) => (await handler.HandleAsync(request, ct)).ToMinimalApiResult()
            )
            .Withvalidation<RegisterUserRequest>()
            .Produces<RegisterUserResponse>();

        group
            .MapPost(
                "verify",
                async (
                    [FromBody] RegisterUserVerifyOtpRequest request,
                    [FromServices] RegisterUserVerifyOtpHandler handler,
                    HttpContext httpcontext,
                    IWebHostEnvironment env,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await handler.HandleAsync(request, cancellationToken);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Error);
                    httpcontext.Login(response.AccesToken, response.Ttl, env);

                    return Results.NoContent();
                }
            )
            .Withvalidation<RegisterUserVerifyOtpRequest>()
            .Produces(204);
    }
}
