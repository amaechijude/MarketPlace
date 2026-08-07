namespace GitgBrand.Api.Infrastructure.OtpValidation;

public sealed record OtpVerificationCode(Guid UserId, OtpType Type, DateTimeOffset ExpiresOn);

public enum OtpType
{
    Register,
    ResetPassword,
}
