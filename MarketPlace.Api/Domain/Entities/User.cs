using Google.Apis.Auth;
using MarketPlace.Api.Common.Normalizer;

namespace MarketPlace.Api.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();
    public string Email { get; private init; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private init; } = string.Empty;
    public bool EmailConfrimed { get; private set; }
    public DateTimeOffset CreatedOn { get; private init; }

    public ICollection<Role> Roles { get; set; } = [];

    public static User Create(string email, DateTimeOffset createdOn)
    {
        return new User
        {
            Email = email,
            NormalizedEmail = EmailNormalizer.Normalize(email),
            CreatedOn = createdOn,
        };
    }

    public void AddPasswordHash(string paswwordHash) => PasswordHash = paswwordHash;

    public void MarkEmailConfirmed() => EmailConfrimed = true;

    public static User Create(GoogleJsonWebSignature.Payload payload, DateTimeOffset createdOn)
    {
        return new User
        {
            Email = payload.Email,
            NormalizedEmail = EmailNormalizer.Normalize(payload.Email),
            CreatedOn = createdOn,
            EmailConfrimed = true,
        };
    }
}
