namespace MarketPlace.Api.Features.Reviews.DTOs;

public sealed record ReviewResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTimeOffset CreatedAt,
    string ReviewerName
);

public sealed record CreateReviewRequest(Guid ProductId, int Rating, string Comment);
