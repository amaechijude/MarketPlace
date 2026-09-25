using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.RateLimiting.Redis;

/// <summary>
/// Distributed, Redis-backed token bucket rate limiter. Intended to be registered as a
/// singleton per (capacity, refillRate, refillIntervalSeconds) configuration — it holds a
/// SemaphoreSlim and cached script hash that are only useful if reused across calls.
/// </summary>
public sealed class TokenBucketLimiter(
    IConnectionMultiplexer connectionMultiplexer,
    int capacity,
    int refillRate,
    int refillIntervalSeconds
) : ITokenBucketLimiter
{
    private readonly IDatabase _db = connectionMultiplexer.GetDatabase();

    private readonly SemaphoreSlim _scriptLoadLock = new(1, 1);

    // Calculated once as a fallback identity; replaced once EnsureScriptLoadedAsync
    // successfully loads the script onto every targeted server.
    private byte[] _scriptHash = SHA1.HashData(Encoding.UTF8.GetBytes(TokenBucketScript));
    private volatile bool _scriptLoaded;

    // Guards against hammering Redis with ScriptLoad retries when loading is persistently
    // failing (e.g. a transient auth/network issue). Cleared as soon as a load succeeds.
    private DateTimeOffset _nextLoadAttempt = DateTimeOffset.MinValue;
    private static readonly TimeSpan LoadRetryCooldown = TimeSpan.FromSeconds(5);

    public async Task<RateLimitResult> AllowAsync(string key, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureScriptLoadedAsync(cancellationToken).ConfigureAwait(false);

        var keys = new RedisKey[] { key };
        var args = new RedisValue[] { capacity, refillRate, refillIntervalSeconds };

        RedisResult result;

        try
        {
            // Try EVALSHA first (faster if script is cached)
            result = await _db.ScriptEvaluateAsync(_scriptHash, keys, args).ConfigureAwait(false);
        }
        catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT"))
        {
            // Script not in cache (e.g. evicted, or FLUSHALL/failover to a fresh node),
            // fall back to EVAL and mark for reload on the next call.
            _scriptLoaded = false;
            result = await _db.ScriptEvaluateAsync(TokenBucketScript, keys, args)
                .ConfigureAwait(false);
        }

        var results = (RedisResult[])result!;
        var allowed = (long)results[0] == 1;
        var remaining = (double)(long)results[1];
        var retryAfterSeconds = (long)results[2];

        return new RateLimitResult(allowed, remaining, retryAfterSeconds);
    }

    /// <summary>
    /// Ensures the Lua script is loaded on the server(s) for EVALSHA usage. Thread-safe: concurrent
    /// callers on first use (or after a NOSCRIPT reset) will wait for a single load attempt rather
    /// than racing each other. Backs off for a short cooldown after a failed attempt so a persistent
    /// load failure doesn't retry on every single request.
    /// </summary>
    private async Task EnsureScriptLoadedAsync(CancellationToken cancellationToken)
    {
        if (_scriptLoaded || DateTimeOffset.UtcNow < _nextLoadAttempt)
            return;

        await _scriptLoadLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_scriptLoaded || DateTimeOffset.UtcNow < _nextLoadAttempt)
                return;

            var servers = _db.Multiplexer.GetServers();
            if (servers.Length == 0)
            {
                // No servers known yet; fall back to EVAL for this call, retry soon.
                _nextLoadAttempt = DateTimeOffset.UtcNow.Add(LoadRetryCooldown);
                return;
            }

            byte[]? loadedHash = null;
            var anyTargeted = false;
            var allSucceeded = true;

            foreach (var server in servers)
            {
                // Loading onto replicas is unnecessary (and can fail on read-only connections);
                // EVALSHA is routed to whichever node serves the key anyway.
                if (server.IsReplica)
                    continue;

                anyTargeted = true;
                try
                {
                    loadedHash = await server
                        .ScriptLoadAsync(TokenBucketScript)
                        .ConfigureAwait(false);
                }
                catch (RedisException)
                {
                    allSucceeded = false;
                }
            }

            var success = anyTargeted && allSucceeded && loadedHash is not null;

            if (loadedHash is not null)
                _scriptHash = loadedHash;

            _scriptLoaded = success;
            _nextLoadAttempt = success
                ? DateTimeOffset.MinValue
                : DateTimeOffset.UtcNow.Add(LoadRetryCooldown);
            // If loading didn't fully succeed, _scriptLoaded stays false and calls fall back to
            // EVAL in the meantime via the NOSCRIPT catch path, while retries are rate-limited.
        }
        finally
        {
            _scriptLoadLock.Release();
        }
    }

    /// <summary>
    /// Lua script for atomic token bucket operations.
    /// All language implementations use this exact script for behavioral consistency.
    /// </summary>
    private const string TokenBucketScript = """

        local key = KEYS[1]
        local capacity = tonumber(ARGV[1])
        local refill_rate = tonumber(ARGV[2])
        local refill_interval = tonumber(ARGV[3])

        -- Use Redis's own clock rather than a value passed from the caller, so that
        -- clock drift between application instances can't skew refill timing.
        local time_result = redis.call('TIME')
        local now = tonumber(time_result[1]) + (tonumber(time_result[2]) / 1000000)

        -- Get current state or initialize
        local bucket = redis.call('HMGET', key, 'tokens', 'last_refill')
        local tokens = tonumber(bucket[1])
        local last_refill = tonumber(bucket[2])

        -- Initialize if this is the first request
        if tokens == nil then
            tokens = capacity
            last_refill = now
        end

        -- Calculate token refill
        local time_passed = now - last_refill
        local refills = math.floor(time_passed / refill_interval)

        if refills > 0 then
            tokens = math.min(capacity, tokens + (refills * refill_rate))
            last_refill = last_refill + (refills * refill_interval)
        end

        -- Try to consume a token
        local allowed = 0
        local retry_after = 0
        if tokens >= 1 then
            tokens = tokens - 1
            allowed = 1
        else
            retry_after = math.ceil(refill_interval - (now - last_refill))
            if retry_after < 0 then
                retry_after = 0
            end
        end

        -- Update state
        redis.call('HMSET', key, 'tokens', tokens, 'last_refill', last_refill)

        -- Expire the key so idle buckets don't accumulate in Redis forever.
        -- TTL covers the time to fully refill from empty, plus one interval of slack.
        local ttl = math.ceil((capacity / refill_rate) * refill_interval) + refill_interval
        redis.call('EXPIRE', key, ttl)

        -- Return result: allowed (1 or 0), remaining tokens, and seconds until next token
        return {allowed, tokens, retry_after}

        """;
}
