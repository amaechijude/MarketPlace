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
                    HttpContext httpContext,
                    IWebHostEnvironment env
                ) =>
                {
                    var response = await handler.HandleAsync(request, httpContext.RequestAborted);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Error);

                    httpContext.Login(response.AccesToken, response.Ttl, env);
                    return Results.NoContent();
                }
            )
            .Withvalidation<EmailLoginRequest>()
            .Produces(204);

        group
            .MapPost(
                "google",
                async (
                    [FromBody] GoogleLoginRequest request,
                    [FromServices] GoogleLoginHandler handler,
                    HttpContext httpContext,
                    IWebHostEnvironment env
                ) =>
                {
                    var response = await handler.HandleAsync(request, httpContext.RequestAborted);
                    if (!response.IsSuccess)
                        return Results.Problem(response.Error);

                    httpContext.Login(response.AccesToken, response.Ttl, env);
                    return Results.NoContent();
                }
            )
            .Withvalidation<GoogleLoginRequest>()
            .Produces(204);
    }
}
