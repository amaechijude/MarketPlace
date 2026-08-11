namespace MarketPlace.Api.Features.Users.ForgotPassword;

public sealed record ForgotPasswordResponse(Guid OtpId, string Message = "Check email for otp");
