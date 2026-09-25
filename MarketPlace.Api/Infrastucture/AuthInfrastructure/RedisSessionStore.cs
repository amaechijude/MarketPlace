// using System.Text.Json;
// using StackExchange.Redis;

// namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

// public sealed class RedisSessionStore(
//     IConnectionMultiplexer connectionMultiplexer,
//     TimeProvider timeProvider
// ) : BaseAuthSessionStore
// {
//     private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

//     public override async Task<(string accessToken, DateTimeOffset expiresOn)> CreateAsync(
//         Guid userId,
//         IEnumerable<string> roles,
//         CancellationToken ct
//     )
//     {
//         var token = GenerateToken();
//         var tokenHash = HashKey(token);

//         var expiresOn = timeProvider.GetUtcNow().Add(CustomAuthSchemeOptions.DefaultTimeSpan);
//         AuthSessionRecord sessionRecord = new(userId, roles, expiresOn);
//         var json = JsonSerializer.Serialize(sessionRecord);

//         var tx = _redis.CreateTransaction();
//         _ = tx.StringSetAsync(tokenHash, json, CustomAuthSchemeOptions.DefaultTimeSpan);
//         _ = tx.SetAddAsync(UserSessionsKey(userId), tokenHash);
//         // Keep the index itself from growing unbounded / outliving its sessions.
//         _ = tx.KeyExpireAsync(UserSessionsKey(userId), CustomAuthSchemeOptions.DefaultTimeSpan);

//         var committed = await tx.ExecuteAsync();
//         if (!committed)
//             throw new InvalidOperationException("Failed to create auth session in Redis.");

//         return (token, expiresOn);
//     }

//     public override async Task<AuthSessionRecord?> GetSessionAsync(
//         string token,
//         CancellationToken ct
//     )
//     {
//         var json = await _redis.StringGetAsync(key: HashKey(token));
//         if (json.IsNullOrEmpty)
//             return null;

//         try
//         {
//             return JsonSerializer.Deserialize<AuthSessionRecord>(json.ToString());
//         }
//         catch (JsonException)
//         {
//             // Corrupt or stale-schema payload — treat as "no valid session"
//             // rather than letting this exception surface from what looks like
//             // a simple lookup. Consider logging this if it recurs, since it
//             // usually signals a deploy-time schema mismatch or bad data.
//             return null;
//         }
//     }

//     public override async Task<(string accessToken, DateTimeOffset expiresOn)> RefreshSessionAsync(
//         string token,
//         AuthSessionRecord record,
//         CancellationToken ct
//     )
//     {
//         // Create the new session before deleting the old one. If anything
//         // fails in between, the caller keeps a working (old) session instead
//         // of being silently logged out. Briefly having two valid tokens is a
//         // much safer failure mode than having zero.
//         var (newToken, expiresOn) = await CreateAsync(record.UserId, record.Roles, ct);
//         await DeleteAsync(token, ct);
//         return (newToken, expiresOn);
//     }

//     public override async Task DeleteAsync(string token, CancellationToken ct)
//     {
//         var tokenHash = HashKey(token);

//         // Need the record first to know which user's index to clean up.
//         var json = await _redis.StringGetAsync(tokenHash);
//         if (json.IsNullOrEmpty)
//         {
//             return;
//         }

//         Guid? userId = null;
//         try
//         {
//             userId = JsonSerializer.Deserialize<AuthSessionRecord>(json.ToString())?.UserId;
//         }
//         catch (JsonException)
//         {
//             // Fall through and still delete the primary key below even if we
//             // can't clean up the index entry for it.
//         }

//         var tx = _redis.CreateTransaction();
//         _ = tx.KeyDeleteAsync(tokenHash);
//         if (userId is { } uid)
//         {
//             _ = tx.SetRemoveAsync(UserSessionsKey(uid), tokenHash);
//         }

//         await tx.ExecuteAsync();
//     }

//     public override async Task DeleteAllUserSessionsAsync(
//         Guid userId,
//         CancellationToken cancellationToken
//     )
//     {
//         var indexKey = UserSessionsKey(userId);
//         var tokenHashes = await _redis.SetMembersAsync(indexKey);

//         if (tokenHashes.Length == 0)
//             return;

//         var keysToDelete = tokenHashes
//             .Select(h => (RedisKey)h.ToString())
//             .Append(indexKey)
//             .ToArray();

//         await _redis.KeyDeleteAsync(keysToDelete);
//     }
// }
