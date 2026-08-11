using System.Net.Mail;
using System.Threading.Channels;
using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using MarketPlace.Api.Infrastucture.OtpValidation;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.Email;

public static class EmailDependencyInjection
{
    public static IServiceCollection AddEmailInfrastructure(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment
    ) => services.AddSmtpConfig().AddEmailRequestChannels().AddEmailSender(hostEnvironment);

    private static IServiceCollection AddEmailRequestChannels(this IServiceCollection services) =>
        services.AddSingleton(
            Channel.CreateBounded<OtpEmailRequest>(
                new BoundedChannelOptions(1000)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false,
                }
            )
        );

    private static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        IWebHostEnvironment hostEnvironment
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

    private static IServiceCollection AddSmtpConfig(this IServiceCollection services)
    {
        services
            .AddOptions<SmtpSettings>()
            .Configure(options =>
            {
                DotNetEnv.Env.TraversePath().Load();

                options.Host =
                    Environment.GetEnvironmentVariable("SMTP_HOST")
                    ?? throw new ArgumentException("SMPTP_HOST mising from config");

                string? port = Environment.GetEnvironmentVariable("SMTP_PORT");

                options.Port = string.IsNullOrWhiteSpace(port)
                    ? throw new ArgumentException("SMPTP_PORT mising from config")
                    : Convert.ToInt32(port);

                options.Username =
                    Environment.GetEnvironmentVariable("SMTP_USERNAME")
                    ?? throw new ArgumentException("SMPTP_USERNAME mising from config");

                options.Password =
                    Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                    ?? throw new ArgumentException("SMPT_PASSWORD mising from config");

                options.FromEmail =
                    Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL")
                    ?? throw new ArgumentException("SMPT_FROM_EMAIL mising from config");

                options.FromName =
                    Environment.GetEnvironmentVariable("SMTP_FROM_NAME")
                    ?? throw new ArgumentException("SMPT_FROM_NAME mising from config");

                options.TimeoutInSeconds = 30;
            })
            .ValidateDataAnnotations()
            .Validate(v => v.Port > 0)
            .ValidateOnStart();

        return services;
    }
}
