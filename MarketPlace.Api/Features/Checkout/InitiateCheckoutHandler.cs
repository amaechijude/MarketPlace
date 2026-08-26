using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Domain.Entities.OwnedTypes;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Checkout;

public sealed class InitiateCheckoutHandler(
    AppDbContext context,
    PaystackApiClient paystackApiClient,
    TimeProvider timeProvider
) : IRequestHandler
{
    private sealed class CheckoutValidationException(string message) : Exception(message);

    public async Task<ApiResponse<CheckoutResponse>> HandleAsync(
        Guid shippingAddressId,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var shippingAddress = await GetShippingAddressAsync(
            userId,
            shippingAddressId,
            cancellationToken
        );
        if (shippingAddress is { AddressSnapshot: null } or { FeeInKobo: <= 0 })
            return ApiResponse<CheckoutResponse>.BadRequest("Invalid shipping address");

        var cart = await context
            .Carts.Where(c => c.UserId == userId)
            .Select(c => new
            {
                c.Id,
                UserEmail = c.User.NormalizedEmail,
                CartItemExists = c.CartItems.Any(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null || !cart.CartItemExists)
            return ApiResponse<CheckoutResponse>.BadRequest("Cart Empty");

        var cartItems = await context
            .CartItems.Include(ci => ci.ProductVariant)
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync(cancellationToken);

        if (cartItems is { Count: 0 })
            return ApiResponse<CheckoutResponse>.BadRequest("Empty cart");

        var orderItems = new List<OrderItem>(capacity: cartItems.Count);
        var now = timeProvider.GetUtcNow();
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CreatedAt = now,
            UserId = userId,
            PaymentReference = $"{Guid.NewGuid()}-{now:u}",
            ShippingFeeInKobo = shippingAddress.FeeInKobo,
        };
        long subtotalAmountInKobo = 0;

        try
        {
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.ProductVariant.StockQuantity)
                    throw new CheckoutValidationException(
                        $"Insufficient stock for {item.ProductVariant.Sku}"
                    );

                subtotalAmountInKobo += item.Quantity * item.ProductVariant.PriceInKobo;
                orderItems.Add(
                    new OrderItem
                    {
                        ProductName = item.ProductVariant.Product.Name,
                        Sku = item.ProductVariant.Sku,
                        Quantity = item.Quantity,
                        UnitPriceInKobo = item.ProductVariant.PriceInKobo,
                        OrderId = order.Id,
                        ProductVariantId = item.ProductVariantId,
                        CreatedAt = now,
                    }
                );
            }

            var totalAmountInKobo = order.AttachSubTotal(subtotalAmountInKobo);
            if (orderItems.Count <= 0 || order.TotalInKobo <= 0)
                return ApiResponse<CheckoutResponse>.BadRequest("Empty order");

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

            context.OrderItems.AddRange(orderItems);
            context.Orders.Add(order);

            await context.SaveChangesAsync(cancellationToken);

            var checkoutResponse = new CheckoutResponse(
                Reference: paystackResponse.Data.Reference,
                AccessCode: paystackResponse.Data.AccessCode,
                AuthorizationUrl: paystackResponse.Data.AuthorizationUrl
            );

            // clear cart
            await context
                .CartItems.Where(c => c.CartId == cart.Id)
                .ExecuteDeleteAsync(cancellationToken);

            return ApiResponse<CheckoutResponse>.Success(checkoutResponse);
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
    }

    private async Task<AddressResult> GetShippingAddressAsync(
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
            return AddressResult.Failed();

        var state = StateNameNormalizer.Normalize(address.State);

        var fee = await context
            .ShippingFees.Where(f => f.NormalizedStateName == state)
            .Select(f => new { f.FeeInKobo })
            .FirstOrDefaultAsync(cancellationToken);

        return fee is null ? AddressResult.Failed() : AddressResult.Succes(address, fee.FeeInKobo);
    }

    private sealed record AddressResult
    {
        public ShippingAddressSnapshot? AddressSnapshot { get; }
        public int FeeInKobo { get; }

        private AddressResult(ShippingAddressSnapshot address, int fee)
        {
            AddressSnapshot = address;
            FeeInKobo = fee;
        }

        private AddressResult()
        {
            AddressSnapshot = null;
        }

        public static AddressResult Succes(ShippingAddressSnapshot address, int fee) =>
            new(address, fee);

        public static AddressResult Failed() => new();
    }
}
