using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Coupons.ApplyCoupon;

public sealed record ApplyCouponRequest(string Code);

public sealed record ApplyCouponResponse(string Message, long DiscountAppliedInKobo);

public sealed class ApplyCouponHandler(AppDbContext dbContext) : IScopedRequestHandler
{
    public async Task<ApiResponse<ApplyCouponResponse>> HandleAsync(
        ApplyCouponRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        var userId = user.UserId;

        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(
            c => c.Code == request.Code,
            cancellationToken
        );

        if (coupon is null || !coupon.IsActive)
        {
            return ApiResponse<ApplyCouponResponse>.NotFound("Coupon not found or inactive");
        }

        var now = DateTimeOffset.UtcNow;
        if (coupon.StartDate.HasValue && coupon.StartDate > now)
        {
            return ApiResponse<ApplyCouponResponse>.BadRequest("Coupon is not yet valid");
        }

        if (coupon.EndDate.HasValue && coupon.EndDate < now)
        {
            return ApiResponse<ApplyCouponResponse>.BadRequest("Coupon has expired");
        }

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
        {
            return ApiResponse<ApplyCouponResponse>.BadRequest("Coupon usage limit reached");
        }

        var cart = await dbContext
            .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null || cart.CartItems.Count == 0)
        {
            return ApiResponse<ApplyCouponResponse>.BadRequest("Cart is empty");
        }

        long totalAmountInKobo = cart.CartItems.Sum(ci =>
            ci.Quantity * ci.ProductVariant.PriceInKobo
        );

        if (
            coupon.MinOrderAmountInKobo.HasValue
            && totalAmountInKobo < coupon.MinOrderAmountInKobo.Value
        )
        {
            return ApiResponse<ApplyCouponResponse>.BadRequest(
                $"Minimum order amount is {coupon.MinOrderAmountInKobo.Value}"
            );
        }

        long discountAmount = 0;
        if (coupon.DiscountType == Domain.Entities.Enums.DiscountType.FixedAmount)
        {
            discountAmount = coupon.DiscountValue;
        }
        else if (coupon.DiscountType == Domain.Entities.Enums.DiscountType.Percentage)
        {
            discountAmount = (long)(totalAmountInKobo * (coupon.DiscountValue / 100.0));
        }

        if (
            coupon.MaxDiscountAmountInKobo.HasValue
            && discountAmount > coupon.MaxDiscountAmountInKobo.Value
        )
        {
            discountAmount = coupon.MaxDiscountAmountInKobo.Value;
        }

        cart.CouponId = coupon.Id;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ApplyCouponResponse>.Success(
            new ApplyCouponResponse("Coupon applied successfully", discountAmount)
        );
    }
}
