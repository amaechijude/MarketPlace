namespace MarketPlace.Api.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordResponse(Guid OtpId, string Message = "Check email for otp");
