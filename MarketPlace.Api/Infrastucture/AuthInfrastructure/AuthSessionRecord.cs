namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

public sealed record AuthSessionRecord(
    Guid UserId,
    IEnumerable<string> Roles,
    DateTimeOffset ExpiresOn
);
