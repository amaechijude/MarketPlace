using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Infrastucture.RateLimiting.Redis;
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
            .AddEndpointFilter(
                async (context, next) =>
                {
                    var limiter =
                        context.HttpContext.RequestServices.GetRequiredService<TokenBucketLimiter>();
                    var userId =
                        context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    var result = limiter.Allow(
                        key: userId + "register",
                        capacity: 5,
                        refillRate: 1,
                        refillIntervalSeconds: 60
                    );

                    if (!result.Allowed)
                        return Results.Problem(statusCode: 429, detail: userId); // Too Many Requests

                    return await next(context);
                }
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
                        return Results.Problem(response.Error);
                    httpResponse.AttachAccessToken(response.AccesToken, response.Ttl, env);

                    return Results.NoContent();
                }
            )
            .WithValidation<RegisterUserVerifyOtpRequest>()
            .Produces(204);
    }
}
