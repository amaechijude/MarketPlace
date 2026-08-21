using MarketPlace.Api.Common.Extensions;
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
                    [FromServices] EmailLoginHandler handler,
                    IWebHostEnvironment env,
                    HttpResponse httpResponse,
                    CancellationToken cancellation
                ) =>
                {
                    LoginResponse response = await handler.HandleAsync(request, cancellation);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Error, statusCode: 400);

                    if (Guid.TryParse(response.AccesToken, out Guid guid))
                        return Results.Ok(new EmailLoginResponse(guid));

                    httpResponse.AttachAccessToken(response.AccesToken, response.Ttl, env);
                    return Results.NoContent();
                }
            )
            .WithValidation<EmailLoginRequest>()
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
                    return Results.Problem(response.Error, statusCode: 400);

                httpResponse.AttachAccessToken(response.AccesToken, response.Ttl, env);
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
                        return Results.Problem(response.Error);

                    httpResponse.AttachAccessToken(response.AccesToken, response.Ttl, env);
                    return Results.NoContent();
                }
            )
            .WithValidation<GoogleLoginRequest>()
            .Produces(204);
    }
}
