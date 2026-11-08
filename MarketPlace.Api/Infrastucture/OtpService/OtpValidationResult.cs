namespace MarketPlace.Api.Infrastucture.OtpService;

public sealed record OtpValidationResult
{
    public bool IsValid { get; }
    public Guid UserId { get; } = Guid.Empty;
    public string Error { get; } = string.Empty;

    public static OtpValidationResult Success(Guid userId) => new(userId);

    public static OtpValidationResult Failed(string error = "Invalid, Expired or Mismatched OTP") =>
        new(error);

    private OtpValidationResult(Guid userId)
    {
        IsValid = true;
        UserId = userId;
    }

    private OtpValidationResult(string error)
    {
        IsValid = false;
        Error = error;
    }
};
