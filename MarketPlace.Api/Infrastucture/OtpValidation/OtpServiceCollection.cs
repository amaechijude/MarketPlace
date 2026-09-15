namespace MarketPlace.Api.Infrastucture.OtpValidation;

public static class OtpServiceCollection
{
    public static IServiceCollection AddOtpInfrastructure(this IServiceCollection services) =>
        services
            .AddHostedService<VerificationCodeBackgroundDispatcher>()
            .AddHostedService<VerificationCodeBackgroundDispatcher1>()
            .AddHostedService<VerificationCodeBackgroundDispatcher2>()
            .AddHostedService<VerificationCodeBackgroundDispatcher3>()
            .AddHostedService<VerificationCodeBackgroundDispatcher4>()
            .AddHostedService<VerificationCodeBackgroundDead>();
}
