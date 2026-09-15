using FluentValidation;
using MarketPlace.Api.Infrastucture.RateLimiting;
using MarketPlace.Api.Infrastucture.RateLimiting.Redis;

namespace MarketPlace.Api.Common.Extensions;

public static class RouteHandlerBuilderExtension
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder WithIpAddressRateLimiter(string prefix) =>
        builder.AddEndpointFilter(async (context, next) =>
        {
            var limiter = context.HttpContext.RequestServices.GetRequiredKeyedService<ITokenBucketLimiter>(IpAddresTokenBucketOptions.Key);

            var result = await limiter.AllowAsync($"prefix:{context.HttpContext.Connection.RemoteIpAddress}");
            if (!result.Allowed)
            {
                context.HttpContext.Response.AttachRetryAfterHeader(result.RetryAfter);
                return Results.Problem(statusCode: StatusCodes.Status429TooManyRequests);
            }
            return await next(context);
        });

        public RouteHandlerBuilder WithValidation<TRequest>()
        =>
            builder.AddEndpointFilter(
                async (context, next) =>
                {
                    var body = context.Arguments.OfType<TRequest>().FirstOrDefault();
                    if (body is null)
                        return Results.Problem(
                            "Missing request body or form",
                            statusCode: 400
                        );

                    // fluent validation
                    var fluent = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
                    if (fluent is not null)
                    {
                        var result = await fluent.ValidateAsync(body, context.HttpContext.RequestAborted);
                        if (!result.IsValid)
                            return Results.ValidationProblem(errors: result.ToDictionary());
                    }
                    return await next(context);
                }
            )
            .ProducesValidationProblem();

        public RouteHandlerBuilder ProducesResponseWithProblem<TResponse>(params ReadOnlySpan<int> errorCodes)
        {
            builder.Produces<TResponse>();

            if (errorCodes is not { Length: > 0 }) return builder;
            foreach (var code in errorCodes) builder.ProducesProblem(code);
            return builder;
        }

    }
}


