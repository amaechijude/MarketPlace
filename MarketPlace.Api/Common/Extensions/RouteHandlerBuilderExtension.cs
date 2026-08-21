using FluentValidation;

namespace MarketPlace.Api.Common.Extensions;

public static class RouteHandlerBuilderExtension
{
    /// <summary>
    /// Validates Against empty request body and fluent validation
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="builder"></param>
    /// <returns>RouteHandlerBuilder</returns>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
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

    /// <summary>
    /// Produces response body with optional error codes problemdetails
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="builder"></param>
    /// <param name="errorCodes"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder ProducesResponsesWithProblem<TResponse>(
        this RouteHandlerBuilder builder,
        List<int>? errorCodes = null
    )
    {
        builder.Produces<TResponse>();

        if (errorCodes is not { Count: > 0 })
            return builder;

        foreach (var code in errorCodes)
        {
            builder.ProducesProblem(code);
        }
        return builder;
    }
}
