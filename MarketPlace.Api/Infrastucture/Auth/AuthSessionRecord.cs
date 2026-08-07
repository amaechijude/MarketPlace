namespace MarketPlace.Api.Infrastucture.Auth;

public sealed record AuthSessionRecord(Guid UserId, IEnumerable<string> Roles);
