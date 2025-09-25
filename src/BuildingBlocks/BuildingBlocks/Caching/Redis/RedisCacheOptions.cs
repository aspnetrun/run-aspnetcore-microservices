using System.Text.Json;

namespace BuildingBlocks.Caching.Redis;

/// <summary>
/// Redis cache configuration options
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// Redis connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
    
    /// <summary>
    /// Redis database ID
    /// </summary>
    public int DatabaseId { get; set; } = 0;
    
    /// <summary>
    /// Default time-to-live for cache items
    /// </summary>
    public TimeSpan? DefaultTimeToLive { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Tag expiration time
    /// </summary>
    public TimeSpan TagExpiration { get; set; } = TimeSpan.FromHours(24);
    
    /// <summary>
    /// Connection timeout
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Operation timeout
    /// </summary>
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// Retry count for failed operations
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Retry delay between attempts
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    
    /// <summary>
    /// Whether to enable connection multiplexing
    /// </summary>
    public bool EnableConnectionMultiplexing { get; set; } = true;
    
    /// <summary>
    /// Maximum connection pool size
    /// </summary>
    public int MaxConnectionPoolSize { get; set; } = 50;
    
    /// <summary>
    /// Whether to enable SSL/TLS
    /// </summary>
    public bool EnableSsl { get; set; } = false;
    
    /// <summary>
    /// SSL host (for certificate validation)
    /// </summary>
    public string? SslHost { get; set; }
    
    /// <summary>
    /// Whether to abort on connection failure
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;
    
    /// <summary>
    /// Whether to enable keep-alive
    /// </summary>
    public bool EnableKeepAlive { get; set; } = true;
    
    /// <summary>
    /// Keep-alive interval
    /// </summary>
    public int KeepAliveInterval { get; set; } = 60;
    
    /// <summary>
    /// Whether to enable performance counters
    /// </summary>
    public bool EnablePerformanceCounters { get; set; } = false;
    
    /// <summary>
    /// JSON serializer options
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
    
    /// <summary>
    /// Whether to enable compression
    /// </summary>
    public bool EnableCompression { get; set; } = false;
    
    /// <summary>
    /// Compression threshold in bytes
    /// </summary>
    public int CompressionThreshold { get; set; } = 1024;
    
    /// <summary>
    /// Whether to enable encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;
    
    /// <summary>
    /// Encryption key
    /// </summary>
    public string? EncryptionKey { get; set; }
    
    /// <summary>
    /// Whether to enable clustering
    /// </summary>
    public bool EnableClustering { get; set; } = false;
    
    /// <summary>
    /// Cluster configuration
    /// </summary>
    public ClusterConfiguration? ClusterConfiguration { get; set; }
}

/// <summary>
/// Redis cluster configuration
/// </summary>
public class ClusterConfiguration
{
    /// <summary>
    /// Cluster name
    /// </summary>
    public string Name { get; set; } = "default";
    
    /// <summary>
    /// Cluster nodes
    /// </summary>
    public List<string> Nodes { get; set; } = new();
    
    /// <summary>
    /// Whether to enable cluster mode
    /// </summary>
    public bool EnableClusterMode { get; set; } = false;
    
    /// <summary>
    /// Cluster timeout
    /// </summary>
    public TimeSpan ClusterTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Whether to enable cluster failover
    /// </summary>
    public bool EnableFailover { get; set; } = true;
    
    /// <summary>
    /// Failover timeout
    /// </summary>
    public TimeSpan FailoverTimeout { get; set; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Redis distributed lock implementation
/// </summary>
public class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _key;
    private readonly string _value;
    private readonly TimeSpan _timeout;
    private bool _disposed;

    public RedisDistributedLock(IDatabase database, string key, string value, TimeSpan timeout)
    {
        _database = database;
        _key = key;
        _value = value;
        _timeout = timeout;
        IsAcquired = true;
        AcquiredAt = DateTime.UtcNow;
    }

    public bool IsAcquired { get; private set; }
    public string Key => _key;
    public TimeSpan Timeout => _timeout;
    public DateTime AcquiredAt { get; }

    public async Task<bool> ExtendAsync(TimeSpan extension)
    {
        if (_disposed || !IsAcquired)
            return false;

        try
        {
            var result = await _database.KeyExpireAsync(_key, extension);
            if (result)
            {
                // Update timeout
                var newTimeout = _timeout + extension;
                // Note: In a real implementation, you'd need to handle this properly
            }
            return result;
        }
        catch
        {
            return false;
        }
    }

    public async Task ReleaseAsync()
    {
        if (_disposed || !IsAcquired)
            return;

        try
        {
            // Use Lua script to ensure we only delete our own lock
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            await _database.ScriptEvaluateAsync(script, new[] { (RedisKey)_key }, new[] { (RedisValue)_value });
            IsAcquired = false;
        }
        catch
        {
            // Ignore errors during release
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _ = ReleaseAsync();
            _disposed = true;
        }
    }
}

/// <summary>
/// Redis subscription implementation
/// </summary>
public class RedisSubscription : IDisposable
{
    private readonly ISubscriber _subscriber;
    private readonly string _channel;
    private bool _disposed;

    public RedisSubscription(ISubscriber subscriber, string channel)
    {
        _subscriber = subscriber;
        _channel = channel;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _ = _subscriber.UnsubscribeAsync(_channel);
            _disposed = true;
        }
    }
}

/// <summary>
/// No-operation disposable for error cases
/// </summary>
public class NoOpDisposable : IDisposable
{
    public void Dispose()
    {
        // Do nothing
    }
}
