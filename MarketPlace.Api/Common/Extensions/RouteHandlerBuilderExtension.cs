using FluentValidation;

namespace MarketPlace.Api.Common.Extensions;

public static class RouteHandlerBuilderExtension
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder WithValidation<TRequest>()
        {
            builder.AddEndpointFilter(
                async (context, next) =>
                {
                    var body = context.Arguments.OfType<TRequest>().FirstOrDefault();
                    if (body is null)
                        return Results.Problem(
                            $"Missing request body or form {nameof(body)}",
                            statusCode: 400
                        );

                    if (body.GetType() != typeof(TRequest))
                        return Results.Problem(
                            $"Request body Mismatch between {nameof(TRequest)} and {body.GetType().Name}",
                            statusCode: 400
                        );

                    // fluent validation
                    var fluent = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
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
            );

            builder.ProducesValidationProblem();
            return builder;
        }

        public RouteHandlerBuilder ProducesResponsesWithProblem<TResponse>(params ReadOnlySpan<int> errorCodes)
        {
            builder.Produces<TResponse>();

            if (errorCodes is not { Length: > 0 })
                return builder;

            foreach (var code in errorCodes)
            {
                builder.ProducesProblem(code);
            }
            return builder;
        }
    }
}
