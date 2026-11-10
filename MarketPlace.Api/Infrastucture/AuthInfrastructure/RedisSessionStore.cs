using JetBrains.Annotations;

namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

public sealed class RedisSessionStore(
    IConnectionMultiplexer mux,
    TimeProvider clock,
    ILogger<RedisSessionStore> logger
) : IAuthSessionStore
{
    private readonly IDatabase _redis = mux.GetDatabase();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const string SessionKeyPrefix = $"{nameof(MarketPlace)}-session:";
    private const string IndexKeyPrefix = $"{nameof(MarketPlace)}-sessions:";

    // Redis layout (one hash per session, no JSON round-trip inside Lua):
    //   data : immutable JSON (userId, roles, createdAt)
    //   uid  : user id, so a single token can be revoked without reading JSON
    //   idle : idle expiry (unix ms)
    //   abs  : absolute expiry (unix ms)
    //   last : last renewal time (unix ms)
    //   rm   : "1" remember-me, "0" otherwise
    //
    // Per-user index: ZSET { member = token hash, score = createdAt ms }.
    //
    // NOTE: these scripts touch keys derived inside Lua (prefix .. member).
    // That is fine on a single node; it would NOT work on Redis Cluster.
    // Do not put '@' characters in Lua comments: LuaScript.Prepare treats
    // @name as a parameter.

    // Atomically validates the session and, if due, renews its idle expiry.
    // Renewal fires when (now - last) >= renewal interval, so a session is
    // never renewed more often than once per interval, and the guaranteed idle
    // tolerance is (timeout - interval).
    private static readonly LuaScript GetAndRenewScript = LuaScript.Prepare(
        """
        local v = redis.call('HMGET', @sessionKey, 'data', 'idle', 'abs', 'last', 'rm')
        if not v[1] then
            return nil
        end

        local now  = tonumber(@now)
        local idle = tonumber(v[2])
        local abs  = tonumber(v[3])
        local last = tonumber(v[4])

        -- Malformed entry: drop it.
        if not idle or not abs or not last then
            redis.call('DEL', @sessionKey)
            return nil
        end

        -- Never renew an expired session.
        if now >= idle or now >= abs then
            redis.call('DEL', @sessionKey)
            return nil
        end

        local rm = (v[5] == '1')
        local timeout
        local interval
        if rm then
            timeout  = tonumber(@rememberIdleTimeout)
            interval = tonumber(@rememberRenewInterval)
        else
            timeout  = tonumber(@idleTimeout)
            interval = tonumber(@renewInterval)
        end

        if now - last >= interval then
            local newIdle = math.min(now + timeout, abs)
            -- Skip no-op writes close to absolute expiry.
            if newIdle > idle then
                idle = newIdle
                last = now
                redis.call('HSET', @sessionKey, 'idle', idle, 'last', last)
                redis.call('PEXPIRE', @sessionKey, idle - now)
            end
        end

        return { v[1], idle, abs, last, rm and 1 or 0 }
        """
    );

    // Creates a session, prunes dead index entries, enforces the per-user cap,
    // and keeps the index key's TTL >= the longest-lived session it references.
    private static readonly LuaScript CreateSessionScript = LuaScript.Prepare(
        """
        -- Drop index entries whose session no longer exists.
        local members = redis.call('ZRANGE', @indexKey, 0, -1)
        for _, m in ipairs(members) do
            if redis.call('EXISTS', @sessionPrefix .. m) == 0 then
                redis.call('ZREM', @indexKey, m)
            end
        end

        redis.call('HSET', @sessionKey,
            'data', @data,
            'uid',  @userId,
            'idle', @idleExpires,
            'abs',  @absExpires,
            'last', @now,
            'rm',   @rm)
        redis.call('PEXPIRE', @sessionKey, @ttl)

        redis.call('ZADD', @indexKey, @now, @tokenHash)

        -- Enforce the cap by revoking the oldest sessions.
        local maxSessions = tonumber(@maxSessions)
        if maxSessions >= 1 then
            local excess = redis.call('ZCARD', @indexKey) - maxSessions
            if excess > 0 then
                local victims = redis.call('ZRANGE', @indexKey, 0, excess - 1)
                for _, m in ipairs(victims) do
                    redis.call('DEL', @sessionPrefix .. m)
                    redis.call('ZREM', @indexKey, m)
                end
            end
        end

        -- Index lives at least as long as the longest session it references.
        local want = tonumber(@absExpires) - tonumber(@now)
        if redis.call('PTTL', @indexKey) < want then
            redis.call('PEXPIRE', @indexKey, want)
        end

        return 1
        """
    );

    // Deletes one session and removes it from its user's index.
    private static readonly LuaScript DeleteSessionScript = LuaScript.Prepare(
        """
        local uid = redis.call('HGET', @sessionKey, 'uid')
        redis.call('DEL', @sessionKey)
        if uid then
            redis.call('ZREM', @indexPrefix .. uid, @tokenHash)
        end
        return 1
        """
    );

    // Deletes every session of a user, then the index itself.
    private static readonly LuaScript DeleteAllSessionsScript = LuaScript.Prepare(
        """
        local members = redis.call('ZRANGE', @indexKey, 0, -1)
        for _, m in ipairs(members) do
            redis.call('DEL', @sessionPrefix .. m)
        end
        redis.call('DEL', @indexKey)
        return #members
        """
    );

    /// <returns>
    /// The opaque token plus two expiries. Do NOT use <c>expiresOn</c> (idle
    /// expiry) as the cookie's Expires: the server renews it. Use a session
    /// cookie for normal logins, and <c>absoluteExpiresOn</c> for remember-me.
    /// </returns>
    public async Task<(
        string accessToken,
        DateTimeOffset expiresOn,
        DateTimeOffset absoluteExpiresOn
    )> CreateAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken ct,
        bool rememberMe = false
    )
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(roles);

        var now = clock.GetUtcNow();

        var idleTimeout = rememberMe
            ? SessionDefaults.RememberMeIdleTimeout
            : SessionDefaults.IdleTimeout;

        var absoluteLifetime = rememberMe
            ? SessionDefaults.RememberMeAbsoluteLifetime
            : SessionDefaults.AbsoluteLifetime;

        var absoluteExpiresOn = now.Add(absoluteLifetime);
        var idleExpiresOn = Min(now.Add(idleTimeout), absoluteExpiresOn);

        var token = GenerateToken();
        var tokenHash = HashKey(token);

        var nowMs = now.ToUnixTimeMilliseconds();
        var data = new SessionData(userId, [.. roles], nowMs);
        var ttl = (long)(idleExpiresOn - now).TotalMilliseconds;

        await _redis
            .ScriptEvaluateAsync(
                CreateSessionScript,
                new
                {
                    sessionKey = (RedisKey)SessionKey(tokenHash),
                    indexKey = (RedisKey)UserSessionsKey(userId),
                    sessionPrefix = SessionKeyPrefix,
                    tokenHash,
                    now = nowMs,
                    data = JsonSerializer.Serialize(data, JsonOptions),
                    userId = userId.ToString(),
                    idleExpires = idleExpiresOn.ToUnixTimeMilliseconds(),
                    absExpires = absoluteExpiresOn.ToUnixTimeMilliseconds(),
                    rm = rememberMe ? "1" : "0",
                    ttl,
                    maxSessions = SessionDefaults.MaxSessionsPerUser,
                }
            )
            .WaitAsync(ct);

        return (token, idleExpiresOn, absoluteExpiresOn);
    }

    public async Task<AuthSessionRecord?> GetSessionAsync(string? token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!IsWellFormedToken(token))
            return null;

        var tokenHash = HashKey(token!);

        var result = await _redis
            .ScriptEvaluateAsync(
                GetAndRenewScript,
                new
                {
                    sessionKey = (RedisKey)SessionKey(tokenHash),
                    now = clock.GetUtcNow().ToUnixTimeMilliseconds(),
                    idleTimeout = (long)SessionDefaults.IdleTimeout.TotalMilliseconds,
                    rememberIdleTimeout = (long)
                        SessionDefaults.RememberMeIdleTimeout.TotalMilliseconds,
                    renewInterval = (long)SessionDefaults.RenewalInterval.TotalMilliseconds,
                    rememberRenewInterval = (long)
                        SessionDefaults.RememberMeRenewalInterval.TotalMilliseconds,
                }
            )
            .WaitAsync(ct);

        if (result.IsNull)
            return null;

        try
        {
            var parts = (RedisResult[])result!;

            var data = JsonSerializer.Deserialize<SessionData>(
                (string?)parts[0] ?? string.Empty,
                JsonOptions
            );

            if (data?.Roles is null)
                throw new JsonException("Session payload is empty.");

            return new AuthSessionRecord(
                UserId: data.UserId,
                Roles: data.Roles,
                CreatedAt: data.CreatedAt,
                LastActivityAt: (long)parts[3],
                IdleExpiresAt: (long)parts[1],
                AbsoluteExpiresAt: (long)parts[2],
                RememberMe: (long)parts[4] == 1
            );
        }
        catch (Exception ex) when (ex is JsonException or InvalidCastException)
        {
            // A corrupt entry should never stay around silently until its TTL.
            logger.LogError(ex, "Corrupt auth session payload; deleting it.");
            await DeleteByHashAsync(tokenHash, ct);
            return null;
        }
    }

    public async Task<(string accessToken, DateTimeOffset expiresOn)> RefreshSessionAsync(
        string token,
        AuthSessionRecord session,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required for a refresh.", nameof(token));

        var (newToken, expiresOn, _) = await CreateAsync(
            session.UserId,
            session.Roles,
            ct,
            session.RememberMe
        );

        await DeleteAsync(token, ct);

        return (newToken, expiresOn);
    }

    public async Task DeleteAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!IsWellFormedToken(token))
            return;

        await DeleteByHashAsync(HashKey(token), ct);
    }

    public async Task DeleteAllUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        await _redis
            .ScriptEvaluateAsync(
                DeleteAllSessionsScript,
                new
                {
                    indexKey = (RedisKey)UserSessionsKey(userId),
                    sessionPrefix = SessionKeyPrefix,
                }
            )
            .WaitAsync(ct);
    }

    private Task DeleteByHashAsync(string tokenHash, CancellationToken ct) =>
        _redis
            .ScriptEvaluateAsync(
                DeleteSessionScript,
                new
                {
                    sessionKey = (RedisKey)SessionKey(tokenHash),
                    indexPrefix = IndexKeyPrefix,
                    tokenHash,
                }
            )
            .WaitAsync(ct);

    private static DateTimeOffset Min(DateTimeOffset first, DateTimeOffset second) =>
        first <= second ? first : second;

    private static string SessionKey(string tokenHash) => $"{SessionKeyPrefix}{tokenHash}";

    private static string UserSessionsKey(Guid userId) => $"{IndexKeyPrefix}{userId}";

    private static string GenerateToken() =>
        Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));

    // Cheap shape check so garbage cookies never reach Redis (or throw in HashKey).
    private static bool IsWellFormedToken(ReadOnlySpan<char> token)
    {
        if (token.IsEmpty || token.IsWhiteSpace())
            return false;
        if (token is not { Length: 64 })
            return false;

        foreach (var c in token)
        {
            if (!char.IsAsciiHexDigitLower(c))
                return false;
        }

        return true;
    }

    private static string HashKey(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        Span<byte> hash = stackalloc byte[32];

        SHA256.HashData(Encoding.UTF8.GetBytes(token), hash);

        return Convert.ToHexStringLower(hash);
    }

    // Immutable part of a session, stored as JSON in the 'data' hash field.
    private sealed record SessionData(Guid UserId, string[] Roles, long CreatedAt);
}

public sealed record AuthSessionRecord(
    Guid UserId,
    IReadOnlyCollection<string> Roles,
    long CreatedAt,
    long LastActivityAt, // time of last renewal, not of last request
    long IdleExpiresAt,
    long AbsoluteExpiresAt,
    bool RememberMe = false
)
{
    [UsedImplicitly]
    [JsonIgnore]
    public DateTimeOffset ExpiresOn =>
        DateTimeOffset.FromUnixTimeMilliseconds(Math.Min(IdleExpiresAt, AbsoluteExpiresAt));

    [UsedImplicitly]
    [JsonIgnore]
    public DateTimeOffset CreatedOn => DateTimeOffset.FromUnixTimeMilliseconds(CreatedAt);

    [UsedImplicitly]
    [JsonIgnore]
    public DateTimeOffset AbsoluteExpiresOn =>
        DateTimeOffset.FromUnixTimeMilliseconds(AbsoluteExpiresAt);
}
