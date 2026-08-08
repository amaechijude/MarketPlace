namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed record OtpEmailRequest(string UserEmail, OtpType Type, string PlainOtp);

public sealed record OtpEmailRazorModel(string Name, string PlainOtp, DateTimeOffset Year);
