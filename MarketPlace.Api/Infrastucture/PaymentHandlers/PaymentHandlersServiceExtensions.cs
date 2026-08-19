using System.Net.Http.Headers;
using MarketPlace.Api.Infrastucture.PaymentHandlers.Paystack;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.PaymentHandlers;

public static class PaymentHandlersServiceExtensions
{
    public static IServiceCollection AddPaymentHandlersInfrastructure(
        this IServiceCollection service,
        IConfiguration configuration
    ) => service.AddPaystackConfig(configuration);

    private static IServiceCollection AddPaystackConfig(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<PayStackSettings>()
            .Bind(configuration.GetRequiredSection(nameof(PayStackSettings)))
            .ValidateDataAnnotations()
            .Validate(v => v.SecretKey.StartsWith("sk_"), "SecretKey must start with 'sk_'")
            .ValidateOnStart();

        services
            .AddHttpClient<PaystackApiClient>(
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<PayStackSettings>>().Value;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        options.SecretKey
                    );
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                    client.Timeout = TimeSpan.FromSeconds(30);
                }
            )
            .AddStandardResilienceHandler();

        return services;
    }
}
