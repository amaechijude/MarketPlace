namespace MarketPlace.Api.Features.Users.Login;

public sealed record LoginResponse
{
    public bool IsSuccess { get; private init; }
    public string AccesToken { get; private init; } = string.Empty;
    public TimeSpan Ttl { get; private init; }
    public string Error { get; private init; } = string.Empty;

    public static LoginResponse Success(string accessToken, TimeSpan ttl) =>
        new()
        {
            IsSuccess = true,
            AccesToken = accessToken,
            Ttl = ttl,
        };

    public static LoginResponse Fail(string error = "Invalid or Expired Otp") =>
        new() { IsSuccess = false, Error = error };
}
