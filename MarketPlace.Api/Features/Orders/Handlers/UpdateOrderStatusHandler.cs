using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Orders.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Orders.Handlers;

public sealed class UpdateOrderStatusHandler(
    AppDbContext context,
    TimeProvider timeProvider,
    ILogger<UpdateOrderStatusHandler> logger
) : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = await context.Orders.FirstOrDefaultAsync(
            o => o.Id == orderId,
            cancellationToken
        );
        if (order is null)
            return ApiResponse<int>.NotFound("Order not found.");

        try
        {
            order.UpdateStatus(request.Status, request.TrackingNumber, timeProvider.GetUtcNow());

            await context.SaveChangesAsync(cancellationToken);
            return ApiResponse<int>.Success(200);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update order status for {OrderId}", orderId);
            return ApiResponse<int>.BadRequest("Failed to update order status.");
        }
    }
}
