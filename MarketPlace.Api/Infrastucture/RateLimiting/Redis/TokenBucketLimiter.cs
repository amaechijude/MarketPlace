using System.Security.Cryptography;
using System.Text;
using MarketPlace.Api.Common.Extensions;
using StackExchange.Redis;

namespace MarketPlace.Api.Infrastucture.RateLimiting.Redis;

public sealed class TokenBucketLimiter(IConnectionMultiplexer connectionMultiplexer)
    : ISingletonMarker
{
    private readonly IDatabase _db = connectionMultiplexer.GetDatabase();

    // Calculate SHA1 of the script for EVALSHA
    private byte[]? _scriptHash = SHA1.HashData(Encoding.UTF8.GetBytes(TokenBucketScript));
    private bool ScriptLoaded = false;

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="capacity"></param>
    /// <param name="refillRate"></param>
    /// <param name="refillIntervalSeconds"></param>
    /// <returns></returns>
    public RateLimitResult Allow(
        string key,
        int capacity,
        int refillRate,
        int refillIntervalSeconds
    )
    {
        EnsureScriptLoaded();

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        var keys = new RedisKey[] { key };
        var args = new RedisValue[] { capacity, refillRate, refillIntervalSeconds, now };

        RedisResult result;

        try
        {
            // Try EVALSHA first (faster if script is cached)
            result = _db.ScriptEvaluate(_scriptHash!, keys, args);
        }
        catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT"))
        {
            // Script not in cache, fall back to EVAL
            result = _db.ScriptEvaluate(TokenBucketScript, keys, args);
            ScriptLoaded = false;
        }

        var results = (RedisResult[])result!;
        var allowed = (long)results[0] == 1;
        var remaining = (double)(long)results[1];

        return new RateLimitResult(allowed, remaining);
    }

    /// <summary>
    /// Ensures the Lua script is loaded into Redis for EVALSHA usage.
    /// </summary>
    private void EnsureScriptLoaded()
    {
        if (ScriptLoaded)
            return;
        try
        {
            var server = _db.Multiplexer.GetServers()[0];
            _scriptHash = server.ScriptLoad(TokenBucketScript);
            ScriptLoaded = true;
        }
        catch (RedisException)
        {
            // If loading fails, we'll fall back to EVAL
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
        local now = tonumber(ARGV[4])

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
        if tokens >= 1 then
            tokens = tokens - 1
            allowed = 1
        end

        -- Update state
        redis.call('HMSET', key, 'tokens', tokens, 'last_refill', last_refill)

        -- Return result: allowed (1 or 0) and remaining tokens
        return {allowed, tokens}

        """;
}
