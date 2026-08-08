using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Common.ApiResponseFactory;

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; }
    public bool IsNoContent { get; }
    public bool IsCreated { get; }
    public T? Data { get; }
    public ProblemDetails Error { get; } = new ProblemDetails { Status = 400 };

    private ApiResponse(T data)
    {
        IsSuccess = true;
        Data = data;
    }

    private ApiResponse(ApiError error)
    {
        ProblemDetails problemDetails = new()
        {
            Detail = error.ErrorDetail,
            Status = error.StatusCode,
        };

        if (error.Fields is { Count: > 0 })
            problemDetails.Extensions["errors"] = error.Fields;

        IsSuccess = false;
        Error = problemDetails;
    }

    private ApiResponse(bool isCreated)
    {
        IsSuccess = true;
        IsNoContent = true;
        IsCreated = isCreated;
    }

    public static ApiResponse<T> Success(T data) => new(data);

    public static ApiResponse<T> Created() => new(true);

    public static ApiResponse<T> NoContent() => new(false);

    // Failure
    private static ApiResponse<T> Failure(ApiError error) => new(error);

    public static ApiResponse<T> BadRequest(string message) =>
        Failure(new ApiError(message, StatusCodes.Status400BadRequest));

    public static ApiResponse<T> ValidationProblem(IDictionary<string, string[]> result) =>
        Failure(
            new ApiError("One Or more validation Failed", StatusCodes.Status400BadRequest, result)
        );

    public static ApiResponse<T> Forbidden(string message = "Permission denied") =>
        Failure(new ApiError(message, StatusCodes.Status403Forbidden));

    public static ApiResponse<T> Unauthorized() =>
        Failure(new ApiError("You need to login", StatusCodes.Status401Unauthorized));

    public static ApiResponse<T> NotFound(string message = "Not found") =>
        Failure(new ApiError(message, StatusCodes.Status404NotFound));

    public static ApiResponse<T> InternalServerError(string message) =>
        Failure(new ApiError(message, StatusCodes.Status500InternalServerError));

    private record struct ApiError(
        string ErrorDetail,
        int StatusCode,
        IDictionary<string, string[]>? Fields = null
    );
}
