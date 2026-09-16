using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Infrastucture.RateLimiting;
using MarketPlace.Api.Infrastucture.RateLimiting.Redis;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Users.Login;

public static class LoginEnpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group = group.MapGroup("login");

        group
            .MapPost(
                "email",
                async (
                    [FromBody] EmailLoginRequest request,
                    [FromKeyedServices(EmailAddresTokenBucketOptions.Key)]
                        ITokenBucketLimiter emailRateLimiter,
                    [FromServices] EmailLoginHandler handler,
                    IWebHostEnvironment env,
                    HttpResponse httpResponse,
                    CancellationToken cancellation
                ) =>
                {
                    var result = await emailRateLimiter.AllowAsync(
                        EmailNormalizer.Normalize(request.Email)
                    );
                    if (!result.Allowed)
                    {
                        httpResponse.AttachRetryAfterHeader(result.RetryAfter);
                        return Results.Problem(statusCode: StatusCodes.Status429TooManyRequests);
                    }
                    var response = await handler.HandleAsync(request, cancellation);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Problem);

                    if (Guid.TryParse(response.AccesToken, out Guid guid))
                        return Results.Ok(new EmailLoginResponse(guid));

                    httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);
                    return Results.NoContent();
                }
            )
            .WithValidation<EmailLoginRequest>()
            .WithIpAddressRateLimiter("login")
            .Produces(204)
            .Produces<EmailLoginResponse>(202);

        group.MapPost(
            "/verify-otp",
            async (
                [FromBody] EmailLoginVerifyOtpRequest request,
                [FromServices] EmailLoginHandler handler,
                IWebHostEnvironment env,
                HttpResponse httpResponse,
                CancellationToken cancellationToken
            ) =>
            {
                LoginResponse response = await handler.VerifyOtpAsync(request, cancellationToken);
                if (!response.IsSuccess)
                    return Results.Problem(response.Problem);

                httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);
                return Results.NoContent();
            }
        );

        group
            .MapPost(
                "google",
                async (
                    [FromBody] GoogleLoginRequest request,
                    [FromServices] GoogleLoginHandler handler,
                    IWebHostEnvironment env,
                    HttpResponse httpResponse,
                    CancellationToken cancellationToken
                ) =>
                {
                    LoginResponse response = await handler.HandleAsync(request, cancellationToken);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Problem);

                    httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);
                    return Results.NoContent();
                }
            )
            .WithValidation<GoogleLoginRequest>()
            .WithIpAddressRateLimiter("google")
            .Produces(204);
    }
}
