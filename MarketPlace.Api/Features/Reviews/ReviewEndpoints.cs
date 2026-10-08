using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Reviews.DTOs;
using MarketPlace.Api.Features.Reviews.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Reviews;

public sealed class ReviewEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/reviews").WithTags("Reviews");

        group
            .MapGet(
                "/product/{productId:guid}",
                async (
                    [FromRoute] Guid productId,
                    [FromQuery] Guid? cursor,
                    [FromQuery] int pageSize,
                    [FromServices] ListProductReviewsHandler handler,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(
                            productId,
                            cursor,
                            pageSize == 0 ? 10 : pageSize,
                            cancellationToken
                        )
                    ).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<CursorPagedResponse<ReviewResponse, Guid?>>(400);

        var protectedGroup = group.RequireAuthorization();

        protectedGroup.MapPost(
            "/",
            async (
                [FromBody] CreateReviewRequest request,
                [FromServices] CreateReviewHandler handler,
                ClaimsPrincipal user,
                CancellationToken cancellationToken
            ) =>
                (
                    await handler.HandleAsync(user.UserId, request, cancellationToken)
                ).ToMinimalApiResult()
        );
    }
}
