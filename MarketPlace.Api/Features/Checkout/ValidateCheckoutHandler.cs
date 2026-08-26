using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities.Enums;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Checkout;

public sealed class ValidateCheckoutHandler(
    AppDbContext context,
    PaystackApiClient paystackApiClient,
    ILogger<ValidateCheckoutHandler> logger
) : IRequestHandler
{
    public async Task<ApiResponse<ValidateCheckoutResponse>> HandleAsync(
        Guid userId,
        string paymentReference,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(paymentReference))
            return ApiResponse<ValidateCheckoutResponse>.BadRequest(
                "Payment reference is required"
            );

        var response = await paystackApiClient.VerifyTransactionAsync(
            paymentReference,
            cancellationToken
        );

        if (response is null)
            return ApiResponse<ValidateCheckoutResponse>.UpstreamServerError(
                "Failed to verify transaction"
            );

        if (!response.Status || response.Data?.Status != "success")
            return ApiResponse<ValidateCheckoutResponse>.BadRequest("Payment verification failed");

        try
        {
            var order =
                await context
                    .Orders.Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                    .Where(o => o.UserId == userId && o.PaymentReference == paymentReference)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new CheckoutValidationException("Order not found");

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
                item.ProductVariant?.StockQuantity -= item.Quantity;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (CheckoutValidationException ex)
        {
            return ApiResponse<ValidateCheckoutResponse>.NotFound(ex.Message);
        }
        catch (CheckoutAlreadyPaidException)
        {
            return ApiResponse<ValidateCheckoutResponse>.BadRequest(
                "Payment already marked as paid"
            );
        }
        // catch (OrderStatusTransitionException ex)
        // {
        //     return ApiResponse<ValidateCheckoutResponse>.BadRequest(ex.Message);
        // }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<ValidateCheckoutResponse>.BadRequest(
                "Too many concurrent attempts to process payment. Please try again."
            );
        }

        return ApiResponse<ValidateCheckoutResponse>.Success(
            new ValidateCheckoutResponse(true, "Payment marked as paid")
        );
    }

    private sealed class CheckoutValidationException(string message) : Exception(message);

    private sealed class CheckoutAlreadyPaidException : Exception;
}
