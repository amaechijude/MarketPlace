namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed record OtpValidationResult(bool IsValid, Guid UserId, string? Error = null)
{
    public static OtpValidationResult Success(Guid userId) => new(true, userId);

    public static OtpValidationResult Failed(string error = "Invalid or Expired otp") =>
        new(false, Guid.Empty, error);
};
