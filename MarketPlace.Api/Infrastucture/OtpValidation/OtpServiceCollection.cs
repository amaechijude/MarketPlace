namespace MarketPlace.Api.Infrastucture.OtpValidation;

public static class OtpServiceCollection
{
    public static IServiceCollection AddOtpInfrastructure(this IServiceCollection services) =>
        services
            .AddHostedService<VerificationCodeBackgroundDispatcher>();
}
