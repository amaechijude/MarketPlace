using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.ShippingAddresses.ListShippingAddress;

public sealed class ListAddressHandler(AppDbContext context) : IScopedRequestHandler
{
    public async Task<ApiResponse<ListAddressResponse>> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        if (userId.IsEmpty)
            return ApiResponse<ListAddressResponse>.Unauthorized();

        var addresses = await context
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

        return ApiResponse<ListAddressResponse>.Success(new ListAddressResponse(addresses));
    }
}
