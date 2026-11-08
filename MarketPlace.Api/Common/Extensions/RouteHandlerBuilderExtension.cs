using FluentValidation;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Features.Auth.Login;
using MarketPlace.Api.Infrastucture.RateLimiting;
using MarketPlace.Api.Infrastucture.RateLimiting.Redis;

namespace MarketPlace.Api.Common.Extensions;

public static class RouteHandlerBuilderExtension
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder WithIpAddressRateLimiter(string prefix) =>
            builder.AddEndpointFilter(
                async (context, next) =>
                {
                    var limiter =
                        context.HttpContext.RequestServices.GetRequiredKeyedService<ITokenBucketLimiter>(
                            IpAddresTokenBucketOptions.Key
                        );

                    var result = await limiter.AllowAsync(
                        $"{prefix}:{context.HttpContext.Connection.RemoteIpAddress}",
                        context.HttpContext.RequestAborted
                    );
                    if (!result.Allowed)
                    {
                        context.HttpContext.Response.AttachRetryAfterHeader(result.RetryAfter);
                        return Results.Problem(statusCode: StatusCodes.Status429TooManyRequests);
                    }
                    return await next(context);
                }
            );

        public RouteHandlerBuilder WithValidation<TRequest>() =>
            builder
                .AddEndpointFilter(
                    async (context, next) =>
                    {
                        var body = context.Arguments.OfType<TRequest>().FirstOrDefault();
                        if (body is null)
                            return Results.Problem("Missing request body or form", statusCode: 400);

                        // fluent validation
                        var fluent = context.HttpContext.RequestServices.GetService<
                            IValidator<TRequest>
                        >();
                        if (fluent is not null)
                        {
                            var result = await fluent.ValidateAsync(
                                body,
                                context.HttpContext.RequestAborted
                            );
                            if (!result.IsValid)
                                return Results.ValidationProblem(errors: result.ToDictionary());
                        }

                        return await next(context);
                    }
                )
                .ProducesValidationProblem();

        public RouteHandlerBuilder WithResponse<TResponse>(
            params ReadOnlySpan<int> otherHttpResponsecodes
        )
        {
            builder.Produces<TResponse>();

            if (otherHttpResponsecodes is not { Length: > 0 })
                return builder;
            foreach (var code in otherHttpResponsecodes)
            {
                if (code >= 400)
                {
                    builder.ProducesProblem(code);
                }
                else
                {
                    builder.Produces(code);
                }
            }
            return builder;
        }

        public RouteHandlerBuilder WithValidationIpAndEmailAddressRateLimit<TRequest>(
            string prefix
        ) =>
            builder.AddEndpointFilter(
                async (context, next) =>
                {
                    var body = context.Arguments.OfType<TRequest>().FirstOrDefault();
                    if (body is null)
                        return Results.Problem("Missing request body or form", statusCode: 400);

                    // fluent validation
                    var fluent = context.HttpContext.RequestServices.GetService<
                        IValidator<TRequest>
                    >();
                    if (fluent is not null)
                    {
                        var res = await fluent.ValidateAsync(
                            body,
                            context.HttpContext.RequestAborted
                        );
                        if (!res.IsValid)
                            return Results.ValidationProblem(errors: res.ToDictionary());
                    }

                    // Rate limit by Ip
                    var limiter =
                        context.HttpContext.RequestServices.GetRequiredKeyedService<ITokenBucketLimiter>(
                            IpAddresTokenBucketOptions.Key
                        );

                    var result = await limiter.AllowAsync(
                        $"{prefix}:{context.HttpContext.Connection.RemoteIpAddress}",
                        context.HttpContext.RequestAborted
                    );
                    if (!result.Allowed)
                    {
                        context.HttpContext.Response.AttachRetryAfterHeader(result.RetryAfter);
                        return Results.Problem(statusCode: StatusCodes.Status429TooManyRequests);
                    }

                    if (
                        body is EmailLoginRequest emailLoginRequest
                        && context
                            .HttpContext.Request.Path.ToString()
                            .Contains("login", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        // email limiter
                        limiter =
                            context.HttpContext.RequestServices.GetRequiredKeyedService<ITokenBucketLimiter>(
                                EmailAddresTokenBucketOptions.Key
                            );
                        result = await limiter.AllowAsync(
                            $"{prefix}:{EmailNormalizer.Normalize(emailLoginRequest.Email)}",
                            context.HttpContext.RequestAborted
                        );
                        if (!result.Allowed)
                        {
                            context.HttpContext.Response.AttachRetryAfterHeader(result.RetryAfter);
                            return Results.Problem(
                                statusCode: StatusCodes.Status429TooManyRequests
                            );
                        }
                    }
                    return await next(context);
                }
            );
    }
}
