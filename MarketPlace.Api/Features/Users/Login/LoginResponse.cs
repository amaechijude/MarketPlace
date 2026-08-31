namespace MarketPlace.Api.Features.Users.Login;

public sealed record LoginResponse
{
    public bool IsSuccess { get; private init; }
    public string AccesToken { get; private init; } = string.Empty;
    public DateTimeOffset ExpiresOn { get; private init; }
    public string Error { get; private init; } = string.Empty;

    public static LoginResponse Success(string accessToken, DateTimeOffset expiresOn) =>
        new()
        {
            IsSuccess = true,
            AccesToken = accessToken,
            ExpiresOn = expiresOn,
        };

    public static LoginResponse Fail(string error = "Invalid or Expired Otp") =>
        new() { IsSuccess = false, Error = error };
}
