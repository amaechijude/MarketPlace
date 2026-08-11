using Microsoft.AspNetCore.Diagnostics;

namespace MarketPlace.Api.Common.ExceptionHandler;

public sealed class GlobalExceptionHandler(
    IWebHostEnvironment env,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statuscode, detail) = Mapexceptions(exception, env);

        logger.LogError(
            exception,
            "Exception was thrown and handled with message {message}",
            exception.Message
        );

        await Results.Problem(detail, statusCode: statuscode).ExecuteAsync(httpContext);
        return true;
    }

    private static (int statuscode, string detail) Mapexceptions(
        Exception exception,
        IWebHostEnvironment env
    )
    {
        return exception switch
        {
            CustomAppExceptions ex => (ex.StatusCode, ex.Message),
            BadHttpRequestException ex => (
                StatusCodes.Status400BadRequest,
                env.IsDevelopment() ? ex.Message : "Bad Request"
            ),
            _ => exception.Message.Contains(
                "AuthorizationPolicy",
                StringComparison.OrdinalIgnoreCase
            )
                ? (StatusCodes.Status403Forbidden, "Forbidden")
                : (
                    StatusCodes.Status500InternalServerError,
                    env.IsDevelopment() ? exception.Message : "Internal server error"
                ),
        };
    }
}
