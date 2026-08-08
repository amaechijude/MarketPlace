using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Api.Common.ApiResponseFactory;

public static class ApiResponseExtension
{
    public static IResult ToMinimalApiResult<T>(this ApiResponse<T> result)
    {
        if (!result.IsSuccess)
            return Results.Problem(result.Error ?? new ProblemDetails { Status = 400 });

        if (result.IsNoContent)
            return result.IsCreated ? Results.Created() : TypedResults.NoContent();

        return Results.Ok(result.Data);
    }
}
