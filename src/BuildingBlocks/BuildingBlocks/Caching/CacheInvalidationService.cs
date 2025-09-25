using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Caching;

/// <summary>
/// Default implementation of cache invalidation service
/// </summary>
public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationService> _logger;
    private readonly Dictionary<string, List<string>> _dependencies = new();
    private readonly List<Func<string, Task>> _invalidationCallbacks = new();
    private readonly InvalidationStatistics _statistics = new();

    public CacheInvalidationService(
        ICacheService cacheService,
        ILogger<CacheInvalidationService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvalidateByPatternAsync(string pattern)
    {
        try
        {
            var keys = await _cacheService.GetKeysAsync(pattern);
            var keysList = keys.ToList();
            
            if (keysList.Any())
            {
                await _cacheService.RemoveMultipleAsync(keysList);
                _statistics.InvalidationsByPattern += keysList.Count;
                _statistics.TotalInvalidations += keysList.Count;
                _statistics.LastInvalidation = DateTime.UtcNow;
                
                _logger.LogInformation("Invalidated {Count} cache entries by pattern: {Pattern}", keysList.Count, pattern);
                
                // Notify callbacks
                await NotifyInvalidationCallbacksAsync(pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task InvalidateByTagAsync(string tag)
    {
        try
        {
            var tagKey = $"tag:{tag}";
            var keys = await _cacheService.GetKeysAsync(tagKey);
            var keysList = keys.ToList();
            
            if (keysList.Any())
            {
                await _cacheService.RemoveMultipleAsync(keysList);
                _statistics.InvalidationsByTag += keysList.Count;
                _statistics.TotalInvalidations += keysList.Count;
                _statistics.LastInvalidation = DateTime.UtcNow;
                
                _logger.LogInformation("Invalidated {Count} cache entries by tag: {Tag}", keysList.Count, tag);
                
                // Notify callbacks
                await NotifyInvalidationCallbacksAsync(tag);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by tag: {Tag}", tag);
        }
    }

    public async Task InvalidateByTagsAsync(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            await InvalidateByTagAsync(tag);
        }
    }

    public async Task InvalidateByPrefixAsync(string prefix)
    {
        try
        {
            var pattern = $"{prefix}*";
            var keys = await _cacheService.GetKeysAsync(pattern);
            var keysList = keys.ToList();
            
            if (keysList.Any())
            {
                await _cacheService.RemoveMultipleAsync(keysList);
                _statistics.InvalidationsByPrefix += keysList.Count;
                _statistics.TotalInvalidations += keysList.Count;
                _statistics.LastInvalidation = DateTime.UtcNow;
                
                _logger.LogInformation("Invalidated {Count} cache entries by prefix: {Prefix}", keysList.Count, prefix);
                
                // Notify callbacks
                await NotifyInvalidationCallbacksAsync(prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by prefix: {Prefix}", prefix);
        }
    }

    public async Task InvalidateBySuffixAsync(string suffix)
    {
        try
        {
            var pattern = $"*{suffix}";
            var keys = await _cacheService.GetKeysAsync(pattern);
            var keysList = keys.ToList();
            
            if (keysList.Any())
            {
                await _cacheService.RemoveMultipleAsync(keysList);
                _statistics.InvalidationsBySuffix += keysList.Count;
                _statistics.TotalInvalidations += keysList.Count;
                _statistics.LastInvalidation = DateTime.UtcNow;
                
                _logger.LogInformation("Invalidated {Count} cache entries by suffix: {Suffix}", keysList.Count, suffix);
                
                // Notify callbacks
                await NotifyInvalidationCallbacksAsync(suffix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by suffix: {Suffix}", suffix);
        }
    }

    public async Task InvalidateByCriteriaAsync(Func<string, bool> criteria)
    {
        try
        {
            var allKeys = await _cacheService.GetKeysAsync("*");
            var keysToInvalidate = allKeys.Where(criteria).ToList();
            
            if (keysToInvalidate.Any())
            {
                await _cacheService.RemoveMultipleAsync(keysToInvalidate);
                _statistics.InvalidationsByCriteria += keysToInvalidate.Count;
                _statistics.TotalInvalidations += keysToInvalidate.Count;
                _statistics.LastInvalidation = DateTime.UtcNow;
                
                _logger.LogInformation("Invalidated {Count} cache entries by custom criteria", keysToInvalidate.Count);
                
                // Notify callbacks
                await NotifyInvalidationCallbacksAsync("custom_criteria");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by custom criteria");
        }
    }

    public async Task AddDependencyAsync(string key, string dependentKey)
    {
        try
        {
            if (!_dependencies.ContainsKey(key))
            {
                _dependencies[key] = new List<string>();
            }
            
            if (!_dependencies[key].Contains(dependentKey))
            {
                _dependencies[key].Add(dependentKey);
                _statistics.TotalDependencies++;
                
                _logger.LogDebug("Added dependency: {Key} -> {DependentKey}", key, dependentKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding dependency: {Key} -> {DependentKey}", key, dependentKey);
        }
    }

    public async Task RemoveDependencyAsync(string key, string dependentKey)
    {
        try
        {
            if (_dependencies.ContainsKey(key))
            {
                var removed = _dependencies[key].Remove(dependentKey);
                if (removed)
                {
                    _statistics.TotalDependencies--;
                    _logger.LogDebug("Removed dependency: {Key} -> {DependentKey}", key, dependentKey);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing dependency: {Key} -> {DependentKey}", key, dependentKey);
        }
    }

    public async Task<IEnumerable<string>> GetDependenciesAsync(string key)
    {
        try
        {
            if (_dependencies.TryGetValue(key, out var deps))
            {
                return deps.AsReadOnly();
            }
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dependencies for key: {Key}", key);
            return Enumerable.Empty<string>();
        }
    }

    public async Task InvalidateWithDependenciesAsync(string key)
    {
        try
        {
            var keysToInvalidate = new List<string> { key };
            
            // Get all dependencies
            if (_dependencies.TryGetValue(key, out var deps))
            {
                keysToInvalidate.AddRange(deps);
            }
            
            // Remove the key and its dependencies
            await _cacheService.RemoveMultipleAsync(keysToInvalidate);
            
            // Remove dependency records
            _dependencies.Remove(key);
            foreach (var dep in deps)
            {
                if (_dependencies.ContainsKey(dep))
                {
                    _dependencies[dep].Remove(key);
                }
            }
            
            _statistics.InvalidationsWithDependencies += keysToInvalidate.Count;
            _statistics.TotalInvalidations += keysToInvalidate.Count;
            _statistics.LastInvalidation = DateTime.UtcNow;
            
            _logger.LogInformation("Invalidated {Count} cache entries with dependencies for key: {Key}", keysToInvalidate.Count, key);
            
            // Notify callbacks
            await NotifyInvalidationCallbacksAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache with dependencies for key: {Key}", key);
        }
    }

    public async Task<IDisposable> RegisterInvalidationCallbackAsync(Func<string, Task> callback)
    {
        try
        {
            _invalidationCallbacks.Add(callback);
            _logger.LogDebug("Registered cache invalidation callback");
            
            return new InvalidationCallbackRegistration(() => _invalidationCallbacks.Remove(callback));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering invalidation callback");
            return new NoOpDisposable();
        }
    }

    public async Task<InvalidationStatistics> GetInvalidationStatisticsAsync()
    {
        return await Task.FromResult(_statistics);
    }

    private async Task NotifyInvalidationCallbacksAsync(string key)
    {
        try
        {
            var tasks = _invalidationCallbacks.Select(callback => callback(key));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying invalidation callbacks for key: {Key}", key);
        }
    }
}

/// <summary>
/// Invalidation callback registration
/// </summary>
public class InvalidationCallbackRegistration : IDisposable
{
    private readonly Action _unregister;
    private bool _disposed;

    public InvalidationCallbackRegistration(Action unregister)
    {
        _unregister = unregister;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _unregister();
            _disposed = true;
        }
    }
}

/// <summary>
/// No-operation disposable
/// </summary>
public class NoOpDisposable : IDisposable
{
    public void Dispose()
    {
        // Do nothing
    }
}
