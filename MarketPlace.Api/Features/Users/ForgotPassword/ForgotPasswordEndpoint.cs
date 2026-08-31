using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Login;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Users.ForgotPassword;

public static class ForgotPasswordEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group = group.MapGroup("forgot-password");

        group
            .MapPost(
                "/",
                async (
                    [FromBody] ForgotPasswordRequest request,
                    [FromServices] ForgotPasswordHandler handler,
                    CancellationToken ct
                ) => (await handler.HandleAsync(request, ct)).ToMinimalApiResult()
            )
            .WithValidation<ForgotPasswordRequest>()
            .Produces<ForgotPasswordResponse>();

        group
            .MapPost(
                "verify",
                async (
                    [FromBody] ResetPasswordRequest request,
                    [FromServices] ResetPasswordHandler handler,
                    HttpResponse httpResponse,
                    IWebHostEnvironment env,
                    CancellationToken cancellationToken
                ) =>
                {
                    LoginResponse response = await handler.HandleAsync(request, cancellationToken);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Error);

                    httpResponse.AttachAccessToken(response.AccesToken, response.ExpiresOn, env);

                    return Results.NoContent();
                }
            )
            .WithValidation<ResetPasswordRequest>()
            .Produces(204);
    }
}
