namespace MarketPlace.Api.Features.Auth;

public sealed record HasOtpResponse(
    string Message = "If the details are valid, a verification code has been sent to your email."
);
