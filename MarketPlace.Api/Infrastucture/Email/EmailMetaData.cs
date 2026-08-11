namespace MarketPlace.Api.Infrastucture.Email;

public sealed record EmailMetaData(
    string ToEmail,
    string Subject,
    string HtmlBody,
    string? ToName = null
);
