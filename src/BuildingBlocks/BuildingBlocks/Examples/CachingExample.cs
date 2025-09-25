using BuildingBlocks.Caching;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Examples;

/// <summary>
/// Example controller showing caching building blocks usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CachingExampleController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly ICacheInvalidationService _invalidationService;
    private readonly ILogger<CachingExampleController> _logger;

    public CachingExampleController(
        ICacheService cacheService,
        IDistributedCacheService distributedCacheService,
        ICacheInvalidationService invalidationService,
        ILogger<CachingExampleController> logger)
    {
        _cacheService = cacheService;
        _distributedCacheService = distributedCacheService;
        _invalidationService = invalidationService;
        _logger = logger;
    }

    /// <summary>
    /// Get a cached value
    /// </summary>
    [HttpGet("get/{key}")]
    public async Task<IActionResult> GetCachedValue(string key)
    {
        var value = await _cacheService.GetAsync<string>(key);
        
        if (value == null)
        {
            return NotFound($"No cached value found for key: {key}");
        }

        return Ok(new { key, value, source = "cache" });
    }

    /// <summary>
    /// Set a cached value
    /// </summary>
    [HttpPost("set")]
    public async Task<IActionResult> SetCachedValue([FromBody] SetCacheRequest request)
    {
        var options = new CacheOptions
        {
            TimeToLive = request.TimeToLive,
            UseSlidingExpiration = request.UseSlidingExpiration,
            Priority = request.Priority,
            Tags = request.Tags,
            Compress = request.Compress,
            Encrypt = request.Encrypt
        };

        await _cacheService.SetAsync(request.Key, request.Value, options);
        
        return Ok(new { message = "Value cached successfully", key = request.Key });
    }

    /// <summary>
    /// Get or set a cached value
    /// </summary>
    [HttpPost("get-or-set")]
    public async Task<IActionResult> GetOrSetCachedValue([FromBody] GetOrSetRequest request)
    {
        var options = new CacheOptions
        {
            TimeToLive = request.TimeToLive,
            Tags = request.Tags
        };

        var value = await _cacheService.GetOrSetAsync(request.Key, async () =>
        {
            _logger.LogInformation("Factory called for key: {Key}", request.Key);
            // Simulate expensive operation
            await Task.Delay(100);
            return $"Generated value for {request.Key} at {DateTime.UtcNow}";
        }, options);

        return Ok(new { key = request.Key, value, source = "cache_or_factory" });
    }

    /// <summary>
    /// Get multiple cached values
    /// </summary>
    [HttpPost("get-multiple")]
    public async Task<IActionResult> GetMultipleCachedValues([FromBody] GetMultipleRequest request)
    {
        var values = await _cacheService.GetMultipleAsync<string>(request.Keys);
        
        var result = values.Select(kvp => new { key = kvp.Key, value = kvp.Value, found = kvp.Value != null });
        
        return Ok(new { keys = request.Keys, results = result });
    }

    /// <summary>
    /// Set multiple cached values
    /// </summary>
    [HttpPost("set-multiple")]
    public async Task<IActionResult> SetMultipleCachedValues([FromBody] SetMultipleRequest request)
    {
        var options = new CacheOptions
        {
            TimeToLive = request.TimeToLive,
            Tags = request.Tags
        };

        await _cacheService.SetMultipleAsync(request.KeyValuePairs, options);
        
        return Ok(new { message = "Multiple values cached successfully", count = request.KeyValuePairs.Count });
    }

    /// <summary>
    /// Increment a numeric value
    /// </summary>
    [HttpPost("increment/{key}")]
    public async Task<IActionResult> IncrementValue(string key, [FromBody] IncrementRequest request)
    {
        var options = new CacheOptions
        {
            TimeToLive = request.TimeToLive
        };

        var newValue = await _cacheService.IncrementAsync(key, request.Value, options);
        
        return Ok(new { key, newValue, operation = "increment" });
    }

    /// <summary>
    /// Decrement a numeric value
    /// </summary>
    [HttpPost("decrement/{key}")]
    public async Task<IActionResult> DecrementValue(string key, [FromBody] DecrementRequest request)
    {
        var options = new CacheOptions
        {
            TimeToLive = request.TimeToLive
        };

        var newValue = await _cacheService.DecrementAsync(key, request.Value, options);
        
        return Ok(new { key, newValue, operation = "decrement" });
    }

    /// <summary>
    /// Remove a cached value
    /// </summary>
    [HttpDelete("remove/{key}")]
    public async Task<IActionResult> RemoveCachedValue(string key)
    {
        var removed = await _cacheService.RemoveAsync(key);
        
        return Ok(new { key, removed, message = removed ? "Value removed successfully" : "Value not found" });
    }

    /// <summary>
    /// Check if a key exists
    /// </summary>
    [HttpGet("exists/{key}")]
    public async Task<IActionResult> CheckKeyExists(string key)
    {
        var exists = await _cacheService.ExistsAsync(key);
        var ttl = await _cacheService.GetTimeToLiveAsync(key);
        
        return Ok(new { key, exists, timeToLive = ttl });
    }

    /// <summary>
    /// Refresh a cached value
    /// </summary>
    [HttpPost("refresh/{key}")]
    public async Task<IActionResult> RefreshCachedValue(string key)
    {
        var refreshed = await _cacheService.RefreshAsync(key);
        
        return Ok(new { key, refreshed, message = refreshed ? "Value refreshed successfully" : "Value not found or cannot be refreshed" });
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetCacheStatistics()
    {
        var stats = await _cacheService.GetStatisticsAsync();
        
        return Ok(stats);
    }

    /// <summary>
    /// Clear all cache
    /// </summary>
    [HttpPost("clear")]
    public async Task<IActionResult> ClearCache()
    {
        await _cacheService.ClearAsync();
        
        return Ok(new { message = "Cache cleared successfully" });
    }

    /// <summary>
    /// Get keys by pattern
    /// </summary>
    [HttpGet("keys/{pattern}")]
    public async Task<IActionResult> GetKeysByPattern(string pattern)
    {
        var keys = await _cacheService.GetKeysAsync(pattern);
        
        return Ok(new { pattern, keys, count = keys.Count() });
    }

    /// <summary>
    /// Invalidate cache by tag
    /// </summary>
    [HttpPost("invalidate/tag")]
    public async Task<IActionResult> InvalidateByTag([FromBody] InvalidateByTagRequest request)
    {
        await _invalidationService.InvalidateByTagAsync(request.Tag);
        
        return Ok(new { message = $"Cache invalidated by tag: {request.Tag}" });
    }

    /// <summary>
    /// Invalidate cache by prefix
    /// </summary>
    [HttpPost("invalidate/prefix")]
    public async Task<IActionResult> InvalidateByPrefix([FromBody] InvalidateByPrefixRequest request)
    {
        await _invalidationService.InvalidateByPrefixAsync(request.Prefix);
        
        return Ok(new { message = $"Cache invalidated by prefix: {request.Prefix}" });
    }

    /// <summary>
    /// Invalidate cache by pattern
    /// </summary>
    [HttpPost("invalidate/pattern")]
    public async Task<IActionResult> InvalidateByPattern([FromBody] InvalidateByPatternRequest request)
    {
        await _invalidationService.InvalidateByPatternAsync(request.Pattern);
        
        return Ok(new { message = $"Cache invalidated by pattern: {request.Pattern}" });
    }

    /// <summary>
    /// Add cache dependency
    /// </summary>
    [HttpPost("dependency")]
    public async Task<IActionResult> AddDependency([FromBody] AddDependencyRequest request)
    {
        await _invalidationService.AddDependencyAsync(request.Key, request.DependentKey);
        
        return Ok(new { message = "Dependency added successfully", key = request.Key, dependentKey = request.DependentKey });
    }

    /// <summary>
    /// Get cache dependencies
    /// </summary>
    [HttpGet("dependency/{key}")]
    public async Task<IActionResult> GetDependencies(string key)
    {
        var dependencies = await _invalidationService.GetDependenciesAsync(key);
        
        return Ok(new { key, dependencies, count = dependencies.Count() });
    }

    /// <summary>
    /// Invalidate with dependencies
    /// </summary>
    [HttpPost("invalidate/with-dependencies")]
    public async Task<IActionResult> InvalidateWithDependencies([FromBody] InvalidateWithDependenciesRequest request)
    {
        await _invalidationService.InvalidateWithDependenciesAsync(request.Key);
        
        return Ok(new { message = $"Cache invalidated with dependencies for key: {request.Key}" });
    }

    /// <summary>
    /// Get invalidation statistics
    /// </summary>
    [HttpGet("invalidation/stats")]
    public async Task<IActionResult> GetInvalidationStatistics()
    {
        var stats = await _invalidationService.GetInvalidationStatisticsAsync();
        
        return Ok(stats);
    }

    /// <summary>
    /// Use distributed lock
    /// </summary>
    [HttpPost("lock")]
    public async Task<IActionResult> UseDistributedLock([FromBody] UseLockRequest request)
    {
        var result = await _distributedCacheService.ExecuteWithLockAsync(request.Key, TimeSpan.FromSeconds(30), async () =>
        {
            _logger.LogInformation("Executing critical section for key: {Key}", request.Key);
            
            // Simulate critical section
            await Task.Delay(1000);
            
            return $"Critical operation completed for {request.Key} at {DateTime.UtcNow}";
        });

        return Ok(new { key = request.Key, result, message = "Critical section executed with distributed lock" });
    }

    /// <summary>
    /// Publish message to channel
    /// </summary>
    [HttpPost("publish")]
    public async Task<IActionResult> PublishMessage([FromBody] PublishMessageRequest request)
    {
        await _distributedCacheService.PublishAsync(request.Channel, request.Message);
        
        return Ok(new { message = "Message published successfully", channel = request.Channel });
    }

    /// <summary>
    /// Get cache health status
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealthStatus()
    {
        var health = await _distributedCacheService.GetHealthStatusAsync();
        
        return Ok(health);
    }

    /// <summary>
    /// Get cluster information
    /// </summary>
    [HttpGet("cluster")]
    public async Task<IActionResult> GetClusterInfo()
    {
        var clusterInfo = await _distributedCacheService.GetClusterInfoAsync();
        
        return Ok(clusterInfo);
    }
}

// Request/Response models
public class SetCacheRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public TimeSpan? TimeToLive { get; set; }
    public bool UseSlidingExpiration { get; set; } = false;
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
    public bool Compress { get; set; } = false;
    public bool Encrypt { get; set; } = false;
}

public class GetOrSetRequest
{
    public string Key { get; set; } = string.Empty;
    public TimeSpan? TimeToLive { get; set; }
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
}

public class GetMultipleRequest
{
    public IEnumerable<string> Keys { get; set; } = Enumerable.Empty<string>();
}

public class SetMultipleRequest
{
    public IDictionary<string, string> KeyValuePairs { get; set; } = new Dictionary<string, string>();
    public TimeSpan? TimeToLive { get; set; }
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();
}

public class IncrementRequest
{
    public long Value { get; set; } = 1;
    public TimeSpan? TimeToLive { get; set; }
}

public class DecrementRequest
{
    public long Value { get; set; } = 1;
    public TimeSpan? TimeToLive { get; set; }
}

public class InvalidateByTagRequest
{
    public string Tag { get; set; } = string.Empty;
}

public class InvalidateByPrefixRequest
{
    public string Prefix { get; set; } = string.Empty;
}

public class InvalidateByPatternRequest
{
    public string Pattern { get; set; } = string.Empty;
}

public class AddDependencyRequest
{
    public string Key { get; set; } = string.Empty;
    public string DependentKey { get; set; } = string.Empty;
}

public class InvalidateWithDependenciesRequest
{
    public string Key { get; set; } = string.Empty;
}

public class UseLockRequest
{
    public string Key { get; set; } = string.Empty;
}

public class PublishMessageRequest
{
    public string Channel { get; set; } = string.Empty;
    public object Message { get; set; } = new();
}
