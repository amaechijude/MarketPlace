using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.Email.Sendly;

public static class SendlyEmailServiceCollection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSendlyEmail(IConfiguration configuration)
        {
            services
                .AddOptions<SendlyEmailOptions>()
                .Bind(configuration.GetRequiredSection(nameof(SendlyEmailOptions)))
                .ValidateDataAnnotations()
                .Validate(
                    x => x.Baseurl.Contains("sendlyai.com"),
                    "Baseurl must be a valid Sendly API url"
                )
                .ValidateOnStart();

            services
                .AddHttpClient<SendlyEmailSender>(
                    (sp, client) =>
                    {
                        var resolve = sp.GetRequiredService<IOptions<SendlyEmailOptions>>().Value;
                        client.BaseAddress = new Uri(resolve.Baseurl);
                        client.DefaultRequestHeaders.Add("Bearer", resolve.ApiKey);
                    }
                )
                .AddStandardResilienceHandler();
            return services;
        }
    }
}
