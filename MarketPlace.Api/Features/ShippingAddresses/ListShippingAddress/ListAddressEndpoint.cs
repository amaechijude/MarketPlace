using System.Security.Claims;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.ShippingAddresses.ListShippingAddress;

public static class ListAddressEndpoint
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder
            .MapGet(
                "/",
                async (
                    [FromServices] AppDbContext context,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                {
                    var userId = user.UserId;
                    if (userId.IsEmpty)
                        return Results.Problem(statusCode: StatusCodes.Status401Unauthorized);

                    var res = await context
                        .ShippingAddresses.AsNoTracking()
                        .Where(s => s.UserId == userId)
                        .OrderByDescending(s => s.CreatedAt)
                        .Take(5)
                        .Select(s => new AddressResponse(
                            s.Id,
                            s.FirstName,
                            s.LastName,
                            s.Phone,
                            s.Address,
                            s.Landmark,
                            s.City,
                            s.State
                        ))
                        .ToListAsync(cancellationToken);

                    return Results.Ok(res);
                }
            )
            .RequireAuthorization()
            .Produces<List<AddressResponse>>();
    }
}
