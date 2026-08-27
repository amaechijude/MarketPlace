using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed record ListShippingFeeResponse(List<ShippingFeeResponse> ShippingFees);

public sealed class GetShippingFeeHandler(AppDbContext context, HybridCache hybridCache)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<ShippingFeeResponse>> HandleAsync(
        int id,
        IEnumerable<string> rolesList,
        CancellationToken cancellationToken
    )
    {
        var cachekey = CacheKeys.ShippingFeeById(id);

        var record = await hybridCache.GetOrCreateAsync(
            key: cachekey,
            factory: async ct =>
            {
                var dbRecord = await context
                    .ShippingFees.AsNoTracking()
                    .Where(f => f.Id == id)
                    .Select(f => new ShippingFeeResponse(f.Id, f.StateName, f.FeeInKobo))
                    .FirstOrDefaultAsync(ct);

                return dbRecord;
            },
            cancellationToken: cancellationToken,
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(10),
                LocalCacheExpiration = TimeSpan.FromHours(1),
            },
            tags: [cachekey]
        );
        return record is null
            ? ApiResponse<ShippingFeeResponse>.NotFound($"Shipping fee for {id} not found.")
            : ApiResponse<ShippingFeeResponse>.Success(record);
    }
}
