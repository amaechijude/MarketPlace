using GitgBrand.Api.Infrastructure.OtpValidation;

namespace MarketPlace.Api.Infrastucture.Email;

public sealed record OtpEmailRequest(
    string UserEmail,
    string PlainOtp,
    string Name,
    string Subject,
    OtpType Type
);
