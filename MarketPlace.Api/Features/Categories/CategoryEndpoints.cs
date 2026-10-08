using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext.SeedData;
using MarketPlace.Api.Features.Categories.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Categories;

public sealed class CategoryEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/categories").WithTags("Categories");

        group
            .MapGet(
                "/",
                async (
                    [AsParameters] ListCategoriesRequest request,
                    [FromServices] ListCategoriesHandler handler,
                    CancellationToken cancellationToken
                ) => (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult()
            )
            .Produces<CursorPagedResponse<CategoryResponse, int?>>();

        group
            .MapPost(
                "/",
                async (
                    [FromBody] CreateCategoryRequest request,
                    [FromServices] CreateCategoryHandler handler,
                    CancellationToken cancellationToken
                ) => (await handler.HandleAsync(request, cancellationToken)).ToMinimalApiResult()
            )
            .RequireAuthorization(p =>
                p.RequireRole(CustomAppRoles.SuperAdmin, CustomAppRoles.Admin)
            )
            .WithValidation<CreateCategoryRequest>()
            .Produces(201);
    }
}
