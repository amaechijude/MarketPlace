namespace MarketPlace.Api.Infrastucture.OtpService;

public static class OtpServiceCollection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddOtpInfrastructure(IConfiguration configuration)
        {
            services
                .AddOptions<OtpOptions>()
                .Bind(configuration.GetRequiredSection(nameof(OtpOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHostedService<OtpEmailWorker>();
            return services;
        }
    }
}
