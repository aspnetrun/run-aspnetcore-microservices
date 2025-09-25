using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace BuildingBlocks.Caching.Redis;

/// <summary>
/// Redis implementation of the distributed cache service
/// </summary>
public class RedisCacheService : IDistributedCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisCacheOptions _options;
    private readonly IDatabase _database;
    private readonly IServer _server;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger,
        IOptions<RedisCacheOptions> options)
    {
        _redis = redis;
        _logger = logger;
        _options = options.Value;
        _database = _redis.GetDatabase(_options.DatabaseId);
        _server = _redis.GetServer(_redis.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value for key: {Key}", key);
            return default;
        }
    }

    public async Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        try
        {
            var keyArray = keys.ToArray();
            var values = await _database.StringGetAsync(keyArray.Select(k => (RedisKey)k).ToArray());
            
            var result = new Dictionary<string, T?>();
            for (int i = 0; i < keyArray.Length; i++)
            {
                result[keyArray[i]] = values[i].HasValue ? Deserialize<T>(values[i]!) : default;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple values for keys: {Keys}", string.Join(", ", keys));
            return keys.ToDictionary(k => k, k => default(T));
        }
    }

    public async Task SetAsync<T>(string key, T value, CacheOptions? options = null)
    {
        try
        {
            var serializedValue = Serialize(value);
            var ttl = options?.TimeToLive ?? _options.DefaultTimeToLive;
            
            if (ttl.HasValue)
            {
                await _database.StringSetAsync(key, serializedValue, ttl.Value);
            }
            else
            {
                await _database.StringSetAsync(key, serializedValue);
            }

            // Store tags if provided
            if (options?.Tags?.Any() == true)
            {
                await StoreTagsAsync(key, options.Tags);
            }

            _logger.LogDebug("Set cache for key: {Key} with TTL: {TTL}", key, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value for key: {Key}", key);
        }
    }

    public async Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, CacheOptions? options = null)
    {
        try
        {
            var batch = _database.CreateBatch();
            var ttl = options?.TimeToLive ?? _options.DefaultTimeToLive;
            
            foreach (var kvp in keyValuePairs)
            {
                var serializedValue = Serialize(kvp.Value);
                if (ttl.HasValue)
                {
                    batch.StringSetAsync(kvp.Key, serializedValue, ttl.Value);
                }
                else
                {
                    batch.StringSetAsync(kvp.Key, serializedValue);
                }
            }

            await batch.ExecuteAsync();
            _logger.LogDebug("Set multiple cache entries: {Count}", keyValuePairs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple values");
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            var result = await _database.KeyDeleteAsync(key);
            await RemoveTagsAsync(key);
            _logger.LogDebug("Removed cache for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key: {Key}", key);
            return false;
        }
    }

    public async Task RemoveMultipleAsync(IEnumerable<string> keys)
    {
        try
        {
            var keyArray = keys.Select(k => (RedisKey)k).ToArray();
            var result = await _database.KeyDeleteAsync(keyArray);
            
            foreach (var key in keys)
            {
                await RemoveTagsAsync(key);
            }

            _logger.LogDebug("Removed multiple cache entries: {Count}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing multiple keys");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        try
        {
            var ttl = await _database.KeyTimeToLiveAsync(key);
            return ttl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return null;
        }
    }

    public async Task<bool> SetTimeToLiveAsync(string key, TimeSpan ttl)
    {
        try
        {
            return await _database.KeyExpireAsync(key, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting TTL for key: {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CacheOptions? options = null)
    {
        try
        {
            var result = await _database.StringIncrementAsync(key, value);
            
            if (options?.TimeToLive.HasValue == true)
            {
                await _database.KeyExpireAsync(key, options.TimeToLive.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing key: {Key}", key);
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1, CacheOptions? options = null)
    {
        try
        {
            var result = await _database.StringDecrementAsync(key, value);
            
            if (options?.TimeToLive.HasValue == true)
            {
                await _database.KeyExpireAsync(key, options.TimeToLive.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing key: {Key}", key);
            return 0;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null)
    {
        var value = await GetAsync<T>(key);
        if (value != null)
            return value;

        // Use distributed lock to prevent multiple factory calls
        using var lockObj = await TryAcquireLockAsync($"{key}:lock", TimeSpan.FromSeconds(30));
        if (lockObj == null)
        {
            // Wait a bit and try to get the value again
            await Task.Delay(100);
            value = await GetAsync<T>(key);
            if (value != null)
                return value;
        }

        // Factory wasn't called, create the value
        value = await factory();
        await SetAsync(key, value, options);
        return value;
    }

    public async Task<bool> RefreshAsync(string key)
    {
        try
        {
            var ttl = await _database.KeyTimeToLiveAsync(key);
            if (ttl.HasValue)
            {
                return await _database.KeyExpireAsync(key, ttl.Value);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing key: {Key}", key);
            return false;
        }
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        try
        {
            var info = await _server.InfoAsync();
            var stats = new CacheStatistics();
            
            // Parse Redis INFO command output
            foreach (var group in info)
            {
                foreach (var entry in group)
                {
                    switch (entry.Key)
                    {
                        case "keyspace":
                            stats.TotalItems = long.Parse(entry.Value);
                            break;
                        case "used_memory":
                            stats.TotalSize = long.Parse(entry.Value);
                            break;
                        case "keyspace_hits":
                            stats.HitCount = long.Parse(entry.Value);
                            break;
                        case "keyspace_misses":
                            stats.MissCount = long.Parse(entry.Value);
                            break;
                        case "evicted_keys":
                            stats.EvictionCount = long.Parse(entry.Value);
                            break;
                        case "expired_keys":
                            stats.ExpirationCount = long.Parse(entry.Value);
                            break;
                    }
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new CacheStatistics();
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _database.ExecuteAsync("FLUSHDB");
            _logger.LogInformation("Cleared all cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        try
        {
            var keys = new List<string>();
            await foreach (var key in _server.KeysAsync(pattern: pattern))
            {
                keys.Add(key.ToString());
            }
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting keys for pattern: {Pattern}", pattern);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IDistributedLock> AcquireLockAsync(string key, TimeSpan timeout)
    {
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var acquired = await _database.StringSetAsync(lockKey, lockValue, timeout, When.NotExists);
        
        if (!acquired)
        {
            throw new InvalidOperationException($"Could not acquire lock for key: {key}");
        }

        return new RedisDistributedLock(_database, lockKey, lockValue, timeout);
    }

    public async Task<IDistributedLock?> TryAcquireLockAsync(string key, TimeSpan timeout)
    {
        try
        {
            var lockKey = $"lock:{key}";
            var lockValue = Guid.NewGuid().ToString();
            var acquired = await _database.StringSetAsync(lockKey, lockValue, timeout, When.NotExists);
            
            if (acquired)
            {
                return new RedisDistributedLock(_database, lockKey, lockValue, timeout);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to acquire lock for key: {Key}", key);
            return null;
        }
    }

    public async Task PublishAsync(string channel, object message)
    {
        try
        {
            var serializedMessage = Serialize(message);
            var subscriberCount = await _database.PublishAsync(channel, serializedMessage);
            _logger.LogDebug("Published message to channel {Channel}, subscribers: {Count}", channel, subscriberCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to channel: {Channel}", channel);
        }
    }

    public async Task<IDisposable> SubscribeAsync(string channel, Func<string, object, Task> handler)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(channel, (_, value) =>
            {
                var message = Deserialize<object>(value);
                _ = Task.Run(async () => await handler(channel, message));
            });

            return new RedisSubscription(subscriber, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to channel: {Channel}", channel);
            return new NoOpDisposable();
        }
    }

    public async Task<long> GetSubscriberCountAsync(string channel)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            var count = await subscriber.PublishAsync(channel, "ping");
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriber count for channel: {Channel}", channel);
            return 0;
        }
    }

    public async Task<bool> SetWithLockAsync<T>(string key, T value, TimeSpan lockTimeout, CacheOptions? options = null)
    {
        using var lockObj = await AcquireLockAsync(key, lockTimeout);
        await SetAsync(key, value, options);
        return true;
    }

    public async Task<T> ExecuteWithLockAsync<T>(string key, TimeSpan lockTimeout, Func<Task<T>> action)
    {
        using var lockObj = await AcquireLockAsync(key, lockTimeout);
        return await action();
    }

    public async Task<CacheHealthStatus> GetHealthStatusAsync()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _database.PingAsync();
            stopwatch.Stop();

            return new CacheHealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                ResponseTime = stopwatch.Elapsed,
                ActiveConnections = _redis.GetCounters().TotalConnectedSockets,
                PendingOperations = 0
            };
        }
        catch (Exception ex)
        {
            return new CacheHealthStatus
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ClusterInfo> GetClusterInfoAsync()
    {
        try
        {
            var endpoints = _redis.GetEndPoints();
            var nodes = new List<NodeInfo>();
            
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var info = await server.InfoAsync();
                
                var nodeInfo = new NodeInfo
                {
                    Id = endpoint.ToString(),
                    Endpoint = endpoint.ToString(),
                    Role = "Master", // Simplified for demo
                    IsConnected = server.IsConnected,
                    LastSeen = DateTime.UtcNow
                };
                
                nodes.Add(nodeInfo);
            }

            return new ClusterInfo
            {
                ClusterName = "Redis Cluster",
                NodeCount = nodes.Count,
                Nodes = nodes,
                PrimaryNode = nodes.FirstOrDefault()?.Id ?? string.Empty,
                IsHealthy = nodes.All(n => n.IsConnected),
                LastHealthCheck = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cluster info");
            return new ClusterInfo
            {
                ClusterName = "Unknown",
                IsHealthy = false
            };
        }
    }

    private async Task StoreTagsAsync(string key, IEnumerable<string> tags)
    {
        try
        {
            foreach (var tag in tags)
            {
                var tagKey = $"tag:{tag}";
                await _database.SetAddAsync(tagKey, key);
                await _database.KeyExpireAsync(tagKey, _options.TagExpiration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing tags for key: {Key}", key);
        }
    }

    private async Task RemoveTagsAsync(string key)
    {
        try
        {
            // This is a simplified implementation
            // In a real scenario, you'd need to track which tags are associated with which keys
            var keys = await GetKeysAsync($"tag:*");
            foreach (var tagKey in keys)
            {
                await _database.SetRemoveAsync(tagKey, key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tags for key: {Key}", key);
        }
    }

    private string Serialize<T>(T value)
    {
        if (value is string str)
            return str;
            
        return JsonSerializer.Serialize(value, _options.JsonSerializerOptions);
    }

    private T Deserialize<T>(string value)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)value;
            
        return JsonSerializer.Deserialize<T>(value, _options.JsonSerializerOptions) ?? default(T)!;
    }
}
