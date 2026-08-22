using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.Cache;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed class UpdateShippingFeeHandler(AppDbContext context, HybridCache hybridCache)
    : IRequestHandler
{
    public async Task<ApiResponse<ShippingFeeResponse>> HandleAsync(
        int id,
        Guid updatedById,
        IEnumerable<string> rolesList,
        UpdatedShippingFeeRequest request,
        CancellationToken cancellationToken
    )
    {
        var record = await context.ShippingFees.FindAsync([id], cancellationToken);
        if (record is null)
        {
            return ApiResponse<ShippingFeeResponse>.NotFound(
                $"Shipping fee with ID {id} not found."
            );
        }

        record.UpdateFee(request.FeeInNaira, updatedById);
        await context.SaveChangesAsync(cancellationToken);

        await hybridCache.RemoveByTagAsync(
            [CacheKeys.ListOfShippingFees, CacheKeys.ShippingFeeById(id)],
            cancellationToken
        );

        return ApiResponse<ShippingFeeResponse>.Success(
            new ShippingFeeResponse(record.Id, record.StateName, record.FeeInKobo)
        );
    }
}
