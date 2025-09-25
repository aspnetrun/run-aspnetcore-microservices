namespace BuildingBlocks.Caching;

/// <summary>
/// Core caching service interface
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Gets multiple values from cache
    /// </summary>
    Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys);
    
    /// <summary>
    /// Sets a value in cache
    /// </summary>
    Task SetAsync<T>(string key, T value, CacheOptions? options = null);
    
    /// <summary>
    /// Sets multiple values in cache
    /// </summary>
    Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, CacheOptions? options = null);
    
    /// <summary>
    /// Removes a value from cache
    /// </summary>
    Task<bool> RemoveAsync(string key);
    
    /// <summary>
    /// Removes multiple values from cache
    /// </summary>
    Task RemoveMultipleAsync(IEnumerable<string> keys);
    
    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// Gets the time-to-live for a key
    /// </summary>
    Task<TimeSpan?> GetTimeToLiveAsync(string key);
    
    /// <summary>
    /// Sets the time-to-live for a key
    /// </summary>
    Task<bool> SetTimeToLiveAsync(string key, TimeSpan ttl);
    
    /// <summary>
    /// Increments a numeric value
    /// </summary>
    Task<long> IncrementAsync(string key, long value = 1, CacheOptions? options = null);
    
    /// <summary>
    /// Decrements a numeric value
    /// </summary>
    Task<long> DecrementAsync(string key, long value = 1, CacheOptions? options = null);
    
    /// <summary>
    /// Gets or sets a value atomically
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null);
    
    /// <summary>
    /// Refreshes the expiration of a key
    /// </summary>
    Task<bool> RefreshAsync(string key);
    
    /// <summary>
    /// Gets cache statistics
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
    
    /// <summary>
    /// Clears all cache
    /// </summary>
    Task ClearAsync();
    
    /// <summary>
    /// Gets all keys matching a pattern
    /// </summary>
    Task<IEnumerable<string>> GetKeysAsync(string pattern);
}

/// <summary>
/// Cache options for controlling behavior
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Time-to-live for the cached item
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }
    
    /// <summary>
    /// Whether to use sliding expiration
    /// </summary>
    public bool UseSlidingExpiration { get; set; } = false;
    
    /// <summary>
    /// Priority for cache eviction
    /// </summary>
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
    
    /// <summary>
    /// Tags for cache invalidation
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
    
    /// <summary>
    /// Whether to compress the value
    /// </summary>
    public bool Compress { get; set; } = false;
    
    /// <summary>
    /// Whether to encrypt the value
    /// </summary>
    public bool Encrypt { get; set; } = false;
    
    /// <summary>
    /// Custom metadata for the cache item
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Cache item priority for eviction policies
/// </summary>
public enum CacheItemPriority
{
    Low,
    Normal,
    High,
    NeverRemove
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public long TotalItems { get; set; }
    public long TotalSize { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
    public long TotalRequests => HitCount + MissCount;
    public long EvictionCount { get; set; }
    public long ExpirationCount { get; set; }
    public Dictionary<string, long> ItemsByType { get; set; } = new();
    public Dictionary<string, long> SizeByType { get; set; } = new();
}
