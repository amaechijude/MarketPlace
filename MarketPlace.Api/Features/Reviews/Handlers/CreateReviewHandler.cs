using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Reviews.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Reviews.Handlers;

public sealed class CreateReviewHandler(AppDbContext context, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        Guid userId,
        CreateReviewRequest request,
        CancellationToken cancellationToken
    )
    {
        var product = await context
            .Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            return ApiResponse<int>.NotFound("Product not found.");

        // Ensure user actually bought the product
        var hasPurchased = await context.OrderItems.AnyAsync(
            oi => oi.ProductVariant!.ProductId == request.ProductId && oi.Order.UserId == userId,
            cancellationToken
        );

        if (!hasPurchased)
            return ApiResponse<int>.BadRequest("You can only review products you have purchased.");

        var existingReview = await context.Reviews.AnyAsync(
            r => r.ProductId == request.ProductId && r.UserId == userId,
            cancellationToken
        );

        if (existingReview)
            return ApiResponse<int>.Conflict("You have already reviewed this product.");

        var review = Review.Create(
            request.ProductId,
            product.VendorId,
            userId,
            request.Rating,
            request.Comment,
            timeProvider.GetUtcNow()
        );

        context.Reviews.Add(review);
        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<int>.Created();
    }
}
