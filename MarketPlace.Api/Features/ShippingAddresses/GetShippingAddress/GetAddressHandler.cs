using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.ShippingAddresses.GetShippingAddress;

public sealed class GetAddressHandler(AppDbContext context, HybridCache hybridCache)
    : IRequestHandler
{
    public async Task<ApiResponse<AddressResponse>> HandleAsync(
        Guid userId,
        Guid addressId,
        CancellationToken cancellationToken
    )
    {
        var address = await hybridCache.GetOrCreateAsync<AddressResponse?>(
            key: CacheKeys.Address(userId),
            factory: async ct =>
                await context
                    .ShippingAddresses.AsNoTracking()
                    .Where(s => s.Id == addressId && s.UserId == userId)
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
}
