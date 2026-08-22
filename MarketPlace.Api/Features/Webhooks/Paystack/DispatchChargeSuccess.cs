using System.Text.Json;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities.Enums;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Webhooks.Paystack;

public sealed class DispatchChargeSuccess(
    AppDbContext context,
    ILogger<DispatchChargeSuccess> logger,
    IHostEnvironment environment
) : IPaystackDispatcher
{
    public async Task HandleAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        var body = payload.Deserialize<PaystackEventData>();

        if (body is null || !IsValidBody(body))
            return;

        try
        {
            var order =
                await context
                    .Orders.Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(
                        o => o.PaymentReference == body.Reference,
                        cancellationToken
                    ) ?? throw new CheckoutValidationException("Order not found");

            if (order.Status is not (OrderStatus.Pending or OrderStatus.Cancelled))
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation(
                        "payment for order {orderid} already cannot be marked paid, current status is {status}",
                        order.Id,
                        order.Status.ToString()
                    );

                throw new CheckoutAlreadyPaidException();
            }

            // Update stock quantities (confirm reservations)
            foreach (var item in order.OrderItems)
            {
                item.Product?.StockQuantity -= item.Quantity;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (CheckoutValidationException ex)
        {
            logger.LogError(ex, "{error}", ex.Message);
        }
        catch (CheckoutAlreadyPaidException)
        {
            logger.LogError("Payment already marked as paid");
        }
        // catch (OrderStatusTransitionException ex)
        // {
        //     return ApiResponse<ValidateCheckoutResponse>.BadRequest(ex.Message);
        // }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Too many concurrent attempts to process payment. Please try again.");
        }
    }

    private bool IsValidBody(PaystackEventData body) =>
        body.Status.Equals("success", StringComparison.OrdinalIgnoreCase)
        && body.Domain.Equals("live", StringComparison.OrdinalIgnoreCase)
        && environment.IsProduction();

    private sealed class CheckoutValidationException(string message) : Exception(message);

    private sealed class CheckoutAlreadyPaidException : Exception;
}
