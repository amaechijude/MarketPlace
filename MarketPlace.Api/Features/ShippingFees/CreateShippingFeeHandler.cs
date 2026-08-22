using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.ShippingFees;

public sealed class CreateShippingFeeHandler(
    AppDbContext context,
    ILogger<CreateShippingFeeHandler> logger
) : IRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid userId,
        CreateShippingFeeRequest request,
        CancellationToken cancellationToken
    )
    {
        var fee = ShippingFee.Create(userId, request.StateName, request.FeeInNaira / 100);
        context.ShippingFees.Add(fee);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Shipping fee for {StateName} state already exists",
                request.StateName
            );
            return ApiResponse<int>.BadRequest(
                $"Shipping fee for {request.StateName} state already exists"
            );
        }

        return ApiResponse<int>.Created();
    }
}
