using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Infrastucture.RateLimiting;
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
            .WithValidation<RegisterUserRequest>()
            .RequireRateLimiting(RateLimitPolicyKeys.RegisterTokenBucket)
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
                        return Results.Problem(response.Error);

                    httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);

                    return Results.NoContent();
                }
            )
            .WithValidation<RegisterUserVerifyOtpRequest>()
            .Produces(204);
        ;
    }
}
