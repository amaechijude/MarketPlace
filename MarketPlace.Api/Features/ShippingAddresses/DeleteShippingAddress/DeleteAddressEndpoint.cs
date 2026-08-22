using System.Security.Claims;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.ShippingAddresses.DeleteShippingAddress;

public class DeleteAddressEndpoint
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder
            .MapDelete(
                "/{addressId:guid}",
                async (
                    Guid addressId,
                    [FromServices] AppDbContext context,
                    ClaimsPrincipal user,
                    CancellationToken ct
                ) =>
                {
                    var userId = user.UserId;
                    if (userId.IsEmpty)
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized);

                    await context
                        .ShippingAddresses.Where(s => s.Id == addressId && s.UserId == userId)
                        .ExecuteDeleteAsync(ct);

                    return Results.NoContent();
                }
            )
            .RequireAuthorization()
            .Produces(204);
    }
}
