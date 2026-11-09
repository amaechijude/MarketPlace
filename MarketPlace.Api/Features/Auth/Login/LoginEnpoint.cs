using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Auth.Login;

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
                    [FromServices] EmailLoginHandler handler,
                    IWebHostEnvironment env,
                    HttpResponse httpResponse,
                    CancellationToken cancellation
                ) =>
                {
                    var response = await handler.HandleAsync(request, cancellation);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Problem);

                    if (response.IsOtpRequired)
                        return Results.Ok(new HasOtpResponse());

                    httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);
                    return Results.NoContent();
                }
            )
            .WithValidationIpAndEmailAddressRateLimit<EmailLoginRequest>("login")
            .Produces(204)
            .Produces<HasOtpResponse>(200);

        group.MapPost(
            "/verify-otp",
            async (
                [FromBody] VerifyOtpApiRequest request,
                [FromServices] EmailLoginHandler handler,
                IWebHostEnvironment env,
                HttpResponse httpResponse,
                CancellationToken cancellationToken
            ) =>
            {
                var response = await handler.VerifyOtpAsync(request, cancellationToken);
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
