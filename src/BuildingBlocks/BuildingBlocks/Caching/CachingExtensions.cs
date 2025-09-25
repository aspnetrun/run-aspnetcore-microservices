using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BuildingBlocks.Caching.Redis;
using StackExchange.Redis;

namespace BuildingBlocks.Caching;

/// <summary>
/// Extension methods for caching services
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds Redis caching services to the service collection
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RedisCacheOptions>? configureOptions = null)
    {
        // Configure Redis options
        var options = new RedisCacheOptions();
        configureOptions?.Invoke(options);
        
        services.Configure<RedisCacheOptions>(config =>
        {
            config.ConnectionString = options.ConnectionString;
            config.DatabaseId = options.DatabaseId;
            config.DefaultTimeToLive = options.DefaultTimeToLive;
            config.TagExpiration = options.TagExpiration;
            config.ConnectionTimeout = options.ConnectionTimeout;
            config.OperationTimeout = options.OperationTimeout;
            config.RetryCount = options.RetryCount;
            config.RetryDelay = options.RetryDelay;
            config.EnableConnectionMultiplexing = options.EnableConnectionMultiplexing;
            config.MaxConnectionPoolSize = options.MaxConnectionPoolSize;
            config.EnableSsl = options.EnableSsl;
            config.SslHost = options.SslHost;
            config.AbortOnConnectFail = options.AbortOnConnectFail;
            config.EnableKeepAlive = options.EnableKeepAlive;
            config.KeepAliveInterval = options.KeepAliveInterval;
            config.EnablePerformanceCounters = options.EnablePerformanceCounters;
            config.JsonSerializerOptions = options.JsonSerializerOptions;
            config.EnableCompression = options.EnableCompression;
            config.CompressionThreshold = options.CompressionThreshold;
            config.EnableEncryption = options.EnableEncryption;
            config.EncryptionKey = options.EncryptionKey;
            config.EnableClustering = options.EnableClustering;
            config.ClusterConfiguration = options.ClusterConfiguration;
        });

        // Register Redis connection multiplexer
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { options.ConnectionString },
                Database = options.DatabaseId,
                ConnectTimeout = (int)options.ConnectionTimeout.TotalMilliseconds,
                SyncTimeout = (int)options.OperationTimeout.TotalMilliseconds,
                ResponseTimeout = (int)options.OperationTimeout.TotalMilliseconds,
                AbortConnect = options.AbortOnConnectFail,
                KeepAlive = options.EnableKeepAlive ? options.KeepAliveInterval : -1,
                ConnectRetry = options.RetryCount,
                ReconnectRetryPolicy = new ExponentialRetry(options.RetryDelay.TotalMilliseconds),
                AllowAdmin = true,
                EnablePerformanceCounters = options.EnablePerformanceCounters
            };

            if (options.EnableSsl)
            {
                configOptions.Ssl = true;
                if (!string.IsNullOrEmpty(options.SslHost))
                {
                    configOptions.SslHost = options.SslHost;
                }
            }

            return ConnectionMultiplexer.Connect(configOptions);
        });

        // Register cache services
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IDistributedCacheService, RedisCacheService>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        
        // Register health check
        services.AddHealthChecks()
            .AddRedis(options.ConnectionString, name: "redis-cache");

        return services;
    }

    /// <summary>
    /// Adds Redis caching with custom configuration section
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection = "Redis")
    {
        var redisOptions = new RedisCacheOptions();
        configuration.GetSection(configurationSection).Bind(redisOptions);
        
        return services.AddRedisCaching(configuration, options =>
        {
            options.ConnectionString = redisOptions.ConnectionString;
            options.DatabaseId = redisOptions.DatabaseId;
            options.DefaultTimeToLive = redisOptions.DefaultTimeToLive;
            options.TagExpiration = redisOptions.TagExpiration;
            options.ConnectionTimeout = redisOptions.ConnectionTimeout;
            options.OperationTimeout = redisOptions.OperationTimeout;
            options.RetryCount = redisOptions.RetryCount;
            options.RetryDelay = redisOptions.RetryDelay;
            options.EnableConnectionMultiplexing = redisOptions.EnableConnectionMultiplexing;
            options.MaxConnectionPoolSize = redisOptions.MaxConnectionPoolSize;
            options.EnableSsl = redisOptions.EnableSsl;
            options.SslHost = redisOptions.SslHost;
            options.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
            options.EnableKeepAlive = redisOptions.EnableKeepAlive;
            options.KeepAliveInterval = redisOptions.KeepAliveInterval;
            options.EnablePerformanceCounters = redisOptions.EnablePerformanceCounters;
            options.JsonSerializerOptions = redisOptions.JsonSerializerOptions;
            options.EnableCompression = redisOptions.EnableCompression;
            options.CompressionThreshold = redisOptions.CompressionThreshold;
            options.EnableEncryption = redisOptions.EnableEncryption;
            options.EncryptionKey = redisOptions.EncryptionKey;
            options.EnableClustering = redisOptions.EnableClustering;
            options.ClusterConfiguration = redisOptions.ClusterConfiguration;
        });
    }

    /// <summary>
    /// Adds Redis caching with custom Redis service implementation
    /// </summary>
    public static IServiceCollection AddRedisCaching<TRedisService>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RedisCacheOptions>? configureOptions = null)
        where TRedisService : class, IDistributedCacheService
    {
        services.AddRedisCaching(configuration, configureOptions);
        services.AddScoped<IDistributedCacheService, TRedisService>();
        return services;
    }

    /// <summary>
    /// Adds Redis caching with custom cache service implementation
    /// </summary>
    public static IServiceCollection AddRedisCaching<TRedisService, TCacheService>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RedisCacheOptions>? configureOptions = null)
        where TRedisService : class, IDistributedCacheService
        where TCacheService : class, ICacheService
    {
        services.AddRedisCaching<TRedisService>(configuration, configureOptions);
        services.AddScoped<ICacheService, TCacheService>();
        return services;
    }

    /// <summary>
    /// Adds in-memory caching as fallback
    /// </summary>
    public static IServiceCollection AddInMemoryCaching(
        this IServiceCollection services,
        Action<MemoryCacheOptions>? configureOptions = null)
    {
        services.AddMemoryCache(configureOptions);
        services.AddScoped<ICacheService, InMemoryCacheService>();
        services.AddScoped<ICacheInvalidationService, InMemoryCacheInvalidationService>();
        return services;
    }

    /// <summary>
    /// Adds hybrid caching (Redis + In-Memory)
    /// </summary>
    public static IServiceCollection AddHybridCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RedisCacheOptions>? configureRedis = null,
        Action<MemoryCacheOptions>? configureMemory = null)
    {
        // Add Redis caching
        services.AddRedisCaching(configuration, configureRedis);
        
        // Add in-memory caching
        services.AddInMemoryCaching(configureMemory);
        
        // Register hybrid cache service
        services.AddScoped<ICacheService, HybridCacheService>();
        
        return services;
    }
}

/// <summary>
/// In-memory cache service implementation
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(IMemoryCache cache, ILogger<InMemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            _cache.TryGetValue(key, out T? value);
            result[key] = value;
        }
        return Task.FromResult(result);
    }

    public Task SetAsync<T>(string key, T value, CacheOptions? options = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions();
        
        if (options?.TimeToLive.HasValue == true)
        {
            if (options.UseSlidingExpiration)
            {
                cacheOptions.SlidingExpiration = options.TimeToLive;
            }
            else
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = options.TimeToLive;
            }
        }

        if (options?.Priority.HasValue == true)
        {
            cacheOptions.Priority = MapPriority(options.Priority.Value);
        }

        _cache.Set(key, value, cacheOptions);
        return Task.CompletedTask;
    }

    public Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, CacheOptions? options = null)
    {
        foreach (var kvp in keyValuePairs)
        {
            SetAsync(kvp.Key, kvp.Value, options);
        }
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.FromResult(true);
    }

    public Task RemoveMultipleAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            _cache.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        // In-memory cache doesn't support TTL queries
        return Task.FromResult<TimeSpan?>(null);
    }

    public Task<bool> SetTimeToLiveAsync(string key, TimeSpan ttl)
    {
        // In-memory cache doesn't support TTL modification
        return Task.FromResult(false);
    }

    public Task<long> IncrementAsync(string key, long value = 1, CacheOptions? options = null)
    {
        var currentValue = _cache.Get<long?>(key) ?? 0;
        var newValue = currentValue + value;
        SetAsync(key, newValue, options);
        return Task.FromResult(newValue);
    }

    public Task<long> DecrementAsync(string key, long value = 1, CacheOptions? options = null)
    {
        var currentValue = _cache.Get<long?>(key) ?? 0;
        var newValue = currentValue - value;
        SetAsync(key, newValue, options);
        return Task.FromResult(newValue);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null)
    {
        var value = await GetAsync<T>(key);
        if (value != null)
            return value;

        value = await factory();
        await SetAsync(key, value, options);
        return value;
    }

    public Task<bool> RefreshAsync(string key)
    {
        // In-memory cache doesn't support refresh
        return Task.FromResult(false);
    }

    public Task<CacheStatistics> GetStatisticsAsync()
    {
        // In-memory cache doesn't provide statistics
        return Task.FromResult(new CacheStatistics());
    }

    public Task ClearAsync()
    {
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        // In-memory cache doesn't support pattern queries
        return Task.FromResult(Enumerable.Empty<string>());
    }

    private static CacheItemPriority MapPriority(CacheItemPriority priority)
    {
        return priority switch
        {
            CacheItemPriority.Low => CacheItemPriority.Low,
            CacheItemPriority.Normal => CacheItemPriority.Normal,
            CacheItemPriority.High => CacheItemPriority.High,
            CacheItemPriority.NeverRemove => CacheItemPriority.NeverRemove,
            _ => CacheItemPriority.Normal
        };
    }
}

/// <summary>
/// In-memory cache invalidation service
/// </summary>
public class InMemoryCacheInvalidationService : ICacheInvalidationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheInvalidationService> _logger;

    public InMemoryCacheInvalidationService(IMemoryCache cache, ILogger<InMemoryCacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task InvalidateByPatternAsync(string pattern)
    {
        // In-memory cache doesn't support pattern invalidation
        _logger.LogWarning("Pattern invalidation not supported in in-memory cache");
        return Task.CompletedTask;
    }

    public Task InvalidateByTagAsync(string tag)
    {
        // In-memory cache doesn't support tag invalidation
        _logger.LogWarning("Tag invalidation not supported in in-memory cache");
        return Task.CompletedTask;
    }

    public Task InvalidateByTagsAsync(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            InvalidateByTagAsync(tag);
        }
        return Task.CompletedTask;
    }

    public Task InvalidateByPrefixAsync(string prefix)
    {
        // In-memory cache doesn't support prefix invalidation
        _logger.LogWarning("Prefix invalidation not supported in in-memory cache");
        return Task.CompletedTask;
    }

    public Task InvalidateBySuffixAsync(string suffix)
    {
        // In-memory cache doesn't support suffix invalidation
        _logger.LogWarning("Suffix invalidation not supported in in-memory cache");
        return Task.CompletedTask;
    }

    public Task InvalidateByCriteriaAsync(Func<string, bool> criteria)
    {
        // In-memory cache doesn't support criteria invalidation
        _logger.LogWarning("Criteria invalidation not supported in in-memory cache");
        return Task.CompletedTask;
    }

    public Task AddDependencyAsync(string key, string dependentKey)
    {
        // In-memory cache doesn't support dependencies
        return Task.CompletedTask;
    }

    public Task RemoveDependencyAsync(string key, string dependentKey)
    {
        // In-memory cache doesn't support dependencies
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetDependenciesAsync(string key)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task InvalidateWithDependenciesAsync(string key)
    {
        // In-memory cache doesn't support dependencies
        return Task.CompletedTask;
    }

    public Task<IDisposable> RegisterInvalidationCallbackAsync(Func<string, Task> callback)
    {
        // In-memory cache doesn't support callbacks
        return Task.FromResult<IDisposable>(new NoOpDisposable());
    }

    public Task<InvalidationStatistics> GetInvalidationStatisticsAsync()
    {
        return Task.FromResult(new InvalidationStatistics());
    }
}

/// <summary>
/// Hybrid cache service (Redis + In-Memory)
/// </summary>
public class HybridCacheService : ICacheService
{
    private readonly IDistributedCacheService _redisCache;
    private readonly ICacheService _memoryCache;
    private readonly ILogger<HybridCacheService> _logger;

    public HybridCacheService(
        IDistributedCacheService redisCache,
        ICacheService memoryCache,
        ILogger<HybridCacheService> logger)
    {
        _redisCache = redisCache;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // Try memory cache first
        var memoryValue = await _memoryCache.GetAsync<T>(key);
        if (memoryValue != null)
        {
            _logger.LogDebug("Cache hit in memory for key: {Key}", key);
            return memoryValue;
        }

        // Try Redis cache
        var redisValue = await _redisCache.GetAsync<T>(key);
        if (redisValue != null)
        {
            // Store in memory cache for faster subsequent access
            await _memoryCache.SetAsync(key, redisValue, new CacheOptions
            {
                TimeToLive = TimeSpan.FromMinutes(5) // Shorter TTL for memory cache
            });
            
            _logger.LogDebug("Cache hit in Redis for key: {Key}", key);
            return redisValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }

    public async Task<IDictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T?>();
        var keysList = keys.ToList();
        
        // Try memory cache first
        var memoryResults = await _memoryCache.GetMultipleAsync<T>(keysList);
        var memoryHits = new List<string>();
        var memoryMisses = new List<string>();
        
        foreach (var kvp in memoryResults)
        {
            if (kvp.Value != null)
            {
                result[kvp.Key] = kvp.Value;
                memoryHits.Add(kvp.Key);
            }
            else
            {
                memoryMisses.Add(kvp.Key);
            }
        }

        // Try Redis for misses
        if (memoryMisses.Any())
        {
            var redisResults = await _redisCache.GetMultipleAsync<T>(memoryMisses);
            foreach (var kvp in redisResults)
            {
                if (kvp.Value != null)
                {
                    result[kvp.Key] = kvp.Value;
                    
                    // Store in memory cache
                    await _memoryCache.SetAsync(kvp.Key, kvp.Value, new CacheOptions
                    {
                        TimeToLive = TimeSpan.FromMinutes(5)
                    });
                }
            }
        }

        return result;
    }

    public async Task SetAsync<T>(string key, T value, CacheOptions? options = null)
    {
        // Set in both caches
        var tasks = new List<Task>
        {
            _memoryCache.SetAsync(key, value, options),
            _redisCache.SetAsync(key, value, options)
        };

        await Task.WhenAll(tasks);
    }

    public async Task SetMultipleAsync<T>(IDictionary<string, T> keyValuePairs, CacheOptions? options = null)
    {
        // Set in both caches
        var tasks = new List<Task>
        {
            _memoryCache.SetMultipleAsync(keyValuePairs, options),
            _redisCache.SetMultipleAsync(keyValuePairs, options)
        };

        await Task.WhenAll(tasks);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        // Remove from both caches
        var tasks = new List<Task<bool>>
        {
            _memoryCache.RemoveAsync(key),
            _redisCache.RemoveAsync(key)
        };

        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    public async Task RemoveMultipleAsync(IEnumerable<string> keys)
    {
        // Remove from both caches
        var tasks = new List<Task>
        {
            _memoryCache.RemoveMultipleAsync(keys),
            _redisCache.RemoveMultipleAsync(keys)
        };

        await Task.WhenAll(tasks);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        // Check memory cache first
        if (await _memoryCache.ExistsAsync(key))
            return true;

        // Check Redis cache
        return await _redisCache.ExistsAsync(key);
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        // Try Redis first (memory cache doesn't support TTL)
        return await _redisCache.GetTimeToLiveAsync(key);
    }

    public async Task<bool> SetTimeToLiveAsync(string key, TimeSpan ttl)
    {
        // Try Redis first (memory cache doesn't support TTL)
        return await _redisCache.SetTimeToLiveAsync(key, ttl);
    }

    public async Task<long> IncrementAsync(string key, long value = 1, CacheOptions? options = null)
    {
        // Use Redis for atomic operations
        var result = await _redisCache.IncrementAsync(key, value, options);
        
        // Update memory cache
        await _memoryCache.SetAsync(key, result, options);
        
        return result;
    }

    public async Task<long> DecrementAsync(string key, long value = 1, CacheOptions? options = null)
    {
        // Use Redis for atomic operations
        var result = await _redisCache.DecrementAsync(key, value, options);
        
        // Update memory cache
        await _memoryCache.SetAsync(key, result, options);
        
        return result;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null)
    {
        // Try to get from cache first
        var value = await GetAsync<T>(key);
        if (value != null)
            return value;

        // Use Redis for distributed locking
        value = await _redisCache.GetOrSetAsync(key, factory, options);
        
        // Store in memory cache
        await _memoryCache.SetAsync(key, value, options);
        
        return value;
    }

    public async Task<bool> RefreshAsync(string key)
    {
        // Try Redis first
        return await _redisCache.RefreshAsync(key);
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        // Combine statistics from both caches
        var redisStats = await _redisCache.GetStatisticsAsync();
        var memoryStats = await _memoryCache.GetStatisticsAsync();
        
        return new CacheStatistics
        {
            TotalItems = redisStats.TotalItems + memoryStats.TotalItems,
            TotalSize = redisStats.TotalSize + memoryStats.TotalSize,
            HitCount = redisStats.HitCount + memoryStats.HitCount,
            MissCount = redisStats.MissCount + memoryStats.MissCount
        };
    }

    public async Task ClearAsync()
    {
        // Clear both caches
        var tasks = new List<Task>
        {
            _memoryCache.ClearAsync(),
            _redisCache.ClearAsync()
        };

        await Task.WhenAll(tasks);
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        // Use Redis for pattern queries
        return await _redisCache.GetKeysAsync(pattern);
    }
}
