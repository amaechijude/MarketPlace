using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Categories.Handlers;

public sealed class CreateCategoryHandler(AppDbContext context, TimeProvider timeProvider)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<int>> HandleAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        var name = Slugger.Slugify(request.Name);

        var existing = await context.Categories.AnyAsync(c => c.Name == name, cancellationToken);
        if (existing)
            return ApiResponse<int>.Conflict("Category already exists.");

        var category = Category.Create(request.Name, timeProvider.GetUtcNow());
        category.DisplayOrder = request.DisplayOrder;

        context.Categories.Add(category);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ApiResponse<int>.Conflict("Category already exists.");
        }
        return ApiResponse<int>.Created();
    }
}
