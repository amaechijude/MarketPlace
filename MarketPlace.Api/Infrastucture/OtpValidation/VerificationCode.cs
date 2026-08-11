namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed record OtpVerificationCode(
    Guid UserId,
    string HashValue,
    OtpType Type,
    DateTimeOffset ExpiresOn
);

public enum OtpType
{
    Register,
    ResetPassword,
    Login,
}
