using System.Net.Mail;
using System.Threading.Channels;
using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using MarketPlace.Api.Infrastucture.OtpService;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.Email;

public static class EmailDependencyInjection
{
    public static IServiceCollection AddEmailInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    ) => services.AddSmtpConfig(configuration).AddEmailSender(hostEnvironment);

    private static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment
    )
    {
        SmtpSettings smtp = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SmtpSettings>>()
            .Value;

        var fluentMalil = services.AddFluentEmail(smtp.FromEmail);
        if (hostEnvironment.IsProduction())
        {
            fluentMalil.AddSmtpSender(smtp.Host, smtp.Port, smtp.Username, smtp.Password);
        }
        else
        {
            fluentMalil.AddSmtpSender(smtp.Host, smtp.Port);
        }

        services.AddSingleton<ISender>(
            new SmtpSender(
                (
                    () =>
                        new SmtpClient()
                        {
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Host = smtp.Host,
                            Port = smtp.Port,
                            EnableSsl = hostEnvironment.IsProduction(),
                        }
                )
            )
        );
        return services;
    }

    private static IServiceCollection AddSmtpConfig(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection("SmtpSettings"))
            .ValidateDataAnnotations()
            .Validate(v => v.Port > 0)
            .ValidateOnStart();

        return services;
    }
}
