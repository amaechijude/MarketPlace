using MarketPlace.Api.Infrastucture.Email.Sendly;
using MarketPlace.Api.Infrastucture.OtpService;

namespace MarketPlace.Api.Infrastucture.Email;

public static class EmailRequestExtension
{
    // public static FluentEmailRequest ToFluentEmailRequest(this OtpEmailRequest request) =>
    //     new(request.UserEmail, request.Subject, request.HtmlBody);

    public static SendlyEmailRequest ToSendlyEmailRequest(
        this OtpEmailRequest request,
        string from
    ) =>
        new()
        {
            ToEmail = [request.UserEmail],
            From = from,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
        };
}
