using System.Text.Json;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Domain.Entities.Enums;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Webhooks.Paystack;

public sealed class DispatchChargeSuccess(
    AppDbContext context,
    ILogger<DispatchChargeSuccess> logger,
    IHostEnvironment environment,
    TimeProvider timeProvider
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
                    .FirstOrDefaultAsync(
                        o => o.PaymentReference == body.Reference,
                        cancellationToken
                    )
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

            var now = timeProvider.GetUtcNow();
            order.UpdateStatus(OrderStatus.ConfirmedPayment, null, now);

            var vendorGroups = order.OrderItems.GroupBy(oi => oi.VendorId);
            foreach (var vendorGroup in vendorGroups)
            {
                var vendorId = vendorGroup.Key;
                var payoutAmount = vendorGroup.Sum(oi => oi.UnitPriceInKobo * oi.Quantity);

                var wallet = await context.Wallets.FirstOrDefaultAsync(
                    w => w.VendorId == vendorId,
                    cancellationToken
                );
                if (wallet is null)
                {
                    wallet = new Wallet { VendorId = vendorId, CreatedAt = now };
                    context.Wallets.Add(wallet);
                }

                wallet.CreditPending(payoutAmount, now);

                context.WalletTransactions.Add(
                    new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Wallet = wallet,
                        AmountInKobo = payoutAmount,
                        Type = TransactionType.Credit,
                        Reference = order.PaymentReference,
                        Description = $"Payment for Order {order.Id}",
                        CreatedAt = now,
                    }
                );
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
