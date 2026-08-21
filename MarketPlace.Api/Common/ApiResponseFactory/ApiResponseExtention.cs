namespace MarketPlace.Api.Common.ApiResponseFactory;

public static class ApiResponseExtension
{
    public static IResult ToMinimalApiResult<T>(this ApiResponse<T> result)
    {
        if (!result.IsSuccess)
            return Results.Problem(result.Error);

        if (result.IsNoContent)
            return result.IsCreated ? Results.Created() : Results.NoContent();

        return Results.Ok(result.Data);
    }
}
