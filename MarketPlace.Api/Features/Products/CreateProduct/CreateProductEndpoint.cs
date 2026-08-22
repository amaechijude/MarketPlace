using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Products.CreateProduct;

public static class CreateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async (
                    [FromForm] CreateProductRequest request,
                    [FromServices] CreateProductHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var (userId, roles) = user.UserIdAndRole;
                    if (userId.IsEmpty)
                        return Results.Problem(statusCode: 401);

                    return (
                        await handler.HandleAsync(request, roles, userId, ct)
                    ).ToMinimalApiResult();
                }
            )
            .RequireAuthorization(p => p.RequireRole("Vendor"))
            .Accepts<CreateProductRequest>("multipart/form-data")
            .WithValidation<CreateProductRequest>()
            .DisableAntiforgery()
            .Produces(StatusCodes.Status201Created);
    }
}
