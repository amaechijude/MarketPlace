using System.Security.Claims;
using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Coupons.ApplyCoupon;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Features.Coupons;

public sealed class CouponsEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("coupons").WithTags("Coupons").RequireAuthorization();

        group
            .MapPost(
                "/apply",
                async (
                    [FromBody] ApplyCouponRequest request,
                    [FromServices] ApplyCouponHandler handler,
                    ClaimsPrincipal user,
                    CancellationToken cancellationToken
                ) =>
                    (
                        await handler.HandleAsync(request, user, cancellationToken)
                    ).ToMinimalApiResult()
            )
            .ProducesResponseWithProblem<ApplyCouponResponse>(400);
    }
}
