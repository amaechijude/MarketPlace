using Microsoft.AspNetCore.Diagnostics;

namespace MarketPlace.Api.Common.ExceptionHandler;

public sealed class GlobalExceptionHandler(IWebHostEnvironment env, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int statuscode, string? detail) = Mapexceptions(exception, env);

        logger.LogError(exception, "Exception was thrown and handled with message {message}", exception.Message);

        await Results.Problem(detail, statusCode: statuscode).ExecuteAsync(httpContext);
        return true;
    }

    private static (int statuscode, string detail) Mapexceptions(Exception ex, IWebHostEnvironment webHostEnvironment)
    {
        return ex is CustomAppExceptions e
            ? (e.StatusCode, e.Message)
            : webHostEnvironment.IsDevelopment()
                ? (StatusCodes.Status500InternalServerError, ex.Message)
                : (StatusCodes.Status500InternalServerError, "Internal server error");
    }
}
