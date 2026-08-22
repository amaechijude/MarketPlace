using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities.Enums;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

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
        {
            return ApiResponse<ValidateCheckoutResponse>.BadRequest(
                "Payment reference is required"
            );
        }

        var cart = await context
            .Carts.Where(c => c.UserId == userId)
            .Select(s => new { s.Id, s.User.NormalizedEmail })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null)
        {
            return ApiResponse<ValidateCheckoutResponse>.NotFound("Cart not found");
        }

        var response = await paystackApiClient.VerifyTransactionAsync(
            paymentReference,
            cancellationToken
        );
        if (response is null)
        {
            return ApiResponse<ValidateCheckoutResponse>.InternalServerError(
                "Failed to verify transaction"
            );
        }

        if (!response.Status || response.Data?.Status != "success")
        {
            return ApiResponse<ValidateCheckoutResponse>.PaymentVerificationFailed(
                "Payment verification failed"
            );
        }

        try
        {
            await Pipeline.ExecuteAsync(
                async ct =>
                {
                    context.ChangeTracker.Clear();

                    var order =
                        await context
                            .Orders.Include(o => o.Payment)
                            .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.ProductVariant)
                            .Where(o =>
                                o.UserId == userId && o.PaymentReference == paymentReference
                            )
                            .FirstOrDefaultAsync(ct)
                        ?? throw new CheckoutValidationException("Order not found");

                    if (order.Status is not (OrderStatus.Pending or OrderStatus.Cancelled))
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation(
                                "payment for order {orderid} already cannot be marked paid, current status is {status}",
                                order.Id,
                                order.Status.ToString()
                            );
                        }

                        throw new CheckoutAlreadyPaidException();
                    }

                    // Update stock quantities (confirm reservations)
                    foreach (var item in order.OrderItems)
                    {
                        item.ProductVariant.ConfirmStockReservation(item.Quantity);
                    }

                    var payment = Payment.Create(
                        orderId: order.Id,
                        reference: paymentReference,
                        amountInKobo: order.TotalInKobo,
                        provider: PaymentProvider.Paystack
                    );

                    if (order.Payment is null)
                    {
                        context.Payments.Add(payment);
                    }

                    order.ConfirmPayment(payment);

                    await context.SaveChangesAsync(ct);
                },
                cancellationToken
            );
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
        catch (OrderStatusTransitionException ex)
        {
            return ApiResponse<ValidateCheckoutResponse>.BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<ValidateCheckoutResponse>.BadRequest(
                "Too many concurrent attempts to process payment. Please try again."
            );
        }

        await ClearCart(cart.Id, cancellationToken);
        // enque emaail

        return ApiResponse<ValidateCheckoutResponse>.Success(
            new ValidateCheckoutResponse(true, "Payment marked as paid")
        );
    }

    private async Task ClearCart(Guid cartId, CancellationToken cancellationToken)
    {
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            var items = await context
                .CartItems.Where(ci => ci.CartId == cartId)
                .ToListAsync(cancellationToken);
            context.CartItems.RemoveRange(items);
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await context
                .CartItems.Where(ci => ci.CartId == cartId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    private static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddRetry(
            new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
            }
        )
        .Build();

    private sealed class CheckoutValidationException(string message) : Exception(message);

    private sealed class CheckoutAlreadyPaidException : Exception;
}
