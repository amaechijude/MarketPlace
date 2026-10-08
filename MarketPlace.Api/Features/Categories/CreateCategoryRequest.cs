using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Features.Categories;

public sealed record CreateCategoryRequest(
    [Required, MaxLength(100), MinLength(4)] string Name,
    [Range(1, int.MaxValue)] int DisplayOrder
);
