using System.Linq.Expressions;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.ShippingAddresses.GetShippingAddress;

public sealed class GetAddressHandler(AppDbContext context, HybridCache hybridCache)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<AddressResponse>> HandleAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken
    )
    {
        if (userId.IsEmpty)
            return ApiResponse<AddressResponse>.Unauthorized();

        var address = await hybridCache.GetOrCreateAsync(
            key: CacheKeys.Address(userId),
            factory: async ct =>
                await context
                    .ShippingAddresses.AsNoTracking()
                    .Where(s => s.Id == addressId && s.UserId == userId)
                    .Select(MapAddress)
                    .FirstOrDefaultAsync(ct),
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(8),
                LocalCacheExpiration = TimeSpan.FromMinutes(4),
            },
            cancellationToken: cancellationToken
        );
        return address is not null
            ? ApiResponse<AddressResponse>.Success(address)
            : ApiResponse<AddressResponse>.NotFound("Address not found");
    }

    private static readonly Expression<Func<ShippingAddress, AddressResponse>> MapAddress =
        s => new AddressResponse(
            s.Id,
            s.FirstName,
            s.LastName,
            s.Phone,
            s.Address,
            s.Landmark,
            s.City,
            s.State
        );
}
