using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed class ListShippingFeeHandler(AppDbContext context, HybridCache hybridCache)
    : IRequestHandler
{
    public async Task<ApiResponse<ListShippingFeeResponse>> HandleAsync(
        IEnumerable<string> rolesList,
        CancellationToken cancellationToken
    )
    {
        var shippingFees = await hybridCache.GetOrCreateAsync(
            key: CacheKeys.ListOfShippingFees,
            factory: async ct =>
            {
                var records = await context
                    .ShippingFees.AsNoTracking()
                    .OrderBy(f => f.Id)
                    .Select(f => new ShippingFeeResponse(f.Id, f.StateName, f.FeeInKobo))
                    .ToListAsync(ct);

                return new ListShippingFeeResponse(records);
            },
            cancellationToken: cancellationToken,
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            },
            tags: [CacheKeys.ListOfShippingFees]
        );

        return ApiResponse<ListShippingFeeResponse>.Success(shippingFees);
    }
}
