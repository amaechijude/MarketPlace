using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Auth.Register;

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
            .WithValidation<RegisterUserRequest>()
            .Produces<RegisterUserResponse>();

        group
            .MapPost(
                "verify",
                async (
                    [FromBody] RegisterUserVerifyOtpRequest request,
                    [FromServices] RegisterUserVerifyOtpHandler handler,
                    IWebHostEnvironment env,
                    HttpResponse httpResponse,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await handler.HandleAsync(request, cancellationToken);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Problem);

                    httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);

                    return Results.NoContent();
                }
            )
            .WithValidation<RegisterUserVerifyOtpRequest>()
            .Produces(204);
        ;
    }
}
