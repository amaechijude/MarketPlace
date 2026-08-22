using System.Net;

namespace MarketPlace.Api.Common.ExceptionHandler;

public abstract class CustomAppExceptions(
    string message,
    HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest
) : Exception(message)
{
    public int StatusCode => (int)httpStatusCode;
}
