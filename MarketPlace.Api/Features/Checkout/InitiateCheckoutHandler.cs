using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Domain.Entities.OwnedTypes;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace MarketPlace.Api.Features.Checkout;

public sealed class InitiateCheckoutHandler(
    AppDbContext context,
    PaystackApiClient paystackApiClient
) : IRequestHandler
{
    private sealed class CheckoutValidationException(string message) : Exception(message);

    public async Task<ApiResponse<CheckoutResponse>> HandleAsync(
        Guid shippingAddressId,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var cart = await context
            .Carts.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new
            {
                c.Id,
                UserEmail = c.User.NormalizedEmail,
                CartItemExists = c.CartItems.Any(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null || !cart.CartItemExists)
            return ApiResponse<CheckoutResponse>.BadRequest("Cart Empty");

        var (shippingAddress, shippingFee) = await GetShippingAddressAsync(
            userId,
            shippingAddressId,
            cancellationToken
        );
        if (shippingAddress is null || shippingFee <= 0)
            return ApiResponse<CheckoutResponse>.BadRequest("Invalid shipping address");

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(
                new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<DbUpdateConcurrencyException>(),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(200),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                }
            )
            .Build();

        Order order = null!;
        long totalAmountInKobo = 0;

        try
        {
            await pipeline.ExecuteAsync(
                async ct =>
                {
                    context.ChangeTracker.Clear();

                    var cartItems = await GetCartItemsAsync(cart.Id, ct);
                    if (cartItems.Count == 0)
                    {
                        throw new CheckoutValidationException("Empty cart");
                    }

                    totalAmountInKobo = 0;
                    List<OrderItem> orderItems = new(cartItems.Count);

                    foreach (var item in cartItems)
                    {
                        var variant = item.ProductVariant;
                        var availableStock = variant.StockQuantity - variant.ReservedQuantity;

                        if (item.Quantity > availableStock)
                        {
                            throw new CheckoutValidationException(
                                $"Insufficient stock for product variant {variant.Sku}"
                            );
                        }

                        // reserve inventory
                        variant.ReserveStock(item.Quantity);
                        totalAmountInKobo += variant.PriceInKobo * item.Quantity;

                        orderItems.Add(
                            OrderItem.Create(
                                productVariantId: variant.Id,
                                productName: variant.Product.Name,
                                sku: variant.Sku,
                                quantity: item.Quantity,
                                unitPrice: variant.PriceInKobo
                            )
                        );
                    }

                    // Add shipping fee to total
                    totalAmountInKobo += shippingFee;

                    order = Order.Create(userId, orderItems, shippingAddress, shippingFee);
                    context.Orders.Add(order);
                    await context.SaveChangesAsync(ct);
                },
                cancellationToken
            );
        }
        catch (CheckoutValidationException ex)
        {
            return ApiResponse<CheckoutResponse>.BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<CheckoutResponse>.BadRequest(
                "Too many concurrent checkout attempts. Please try again."
            );
        }

        var paystackInitPayload = new PaystackInitPayload
        {
            Email = cart.UserEmail,
            AmountInKobo = totalAmountInKobo.ToString(),
            Reference = order.PaymentReference,
        };

        var paystackResponse = await paystackApiClient.InitializeTransactionAsync(
            paystackInitPayload,
            cancellationToken
        );
        if (paystackResponse is null)
            return ApiResponse<CheckoutResponse>.BadRequest("Failed to initialize payment");

        var checkoutResponse = new CheckoutResponse(
            Reference: paystackResponse.Data.Reference,
            AccessCode: paystackResponse.Data.AccessCode,
            AuthorizationUrl: paystackResponse.Data.AuthorizationUrl
        );

        return ApiResponse<CheckoutResponse>.Success(checkoutResponse);
    }

    private async Task<List<CartItem>> GetCartItemsAsync(
        Guid cartId,
        CancellationToken cancellationToken
    )
    {
        return await context
            .CartItems.Include(ci => ci.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Where(ci => ci.CartId == cartId && ci.ProductVariant.StockQuantity > 0)
            .ToListAsync(cancellationToken);
    }

    private async Task<(ShippingAddressSnapshot?, int)> GetShippingAddressAsync(
        Guid userId,
        Guid shippingAddressId,
        CancellationToken cancellationToken
    )
    {
        var address = await context
            .ShippingAddresses.AsNoTracking()
            .Where(s => s.Id == shippingAddressId && s.UserId == userId)
            .Select(s => new ShippingAddressSnapshot(
                Id: s.Id,
                FirstName: s.FirstName,
                LastName: s.LastName,
                Phone: s.Phone,
                Address: s.Address,
                Landmark: s.Landmark,
                City: s.City,
                State: s.State
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (address is null)
            return (null, 0);

        var state = address.State.Trim().ToLower();
        var fee = await context
            .ShippingFees.Where(f => f.StateName.ToLower() == state)
            .Select(f => f.FeeInKobo)
            .FirstOrDefaultAsync(cancellationToken);

        return (address, fee);
    }
}

public sealed record CheckoutResponse(string Reference, string AccessCode, string AuthorizationUrl);
