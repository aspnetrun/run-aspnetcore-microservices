namespace BuildingBlocks.Caching;

/// <summary>
/// Service for cache invalidation and dependency management
/// </summary>
public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates cache by pattern
    /// </summary>
    Task InvalidateByPatternAsync(string pattern);
    
    /// <summary>
    /// Invalidates cache by tag
    /// </summary>
    Task InvalidateByTagAsync(string tag);
    
    /// <summary>
    /// Invalidates cache by multiple tags
    /// </summary>
    Task InvalidateByTagsAsync(IEnumerable<string> tags);
    
    /// <summary>
    /// Invalidates cache by key prefix
    /// </summary>
    Task InvalidateByPrefixAsync(string prefix);
    
    /// <summary>
    /// Invalidates cache by key suffix
    /// </summary>
    Task InvalidateBySuffixAsync(string suffix);
    
    /// <summary>
    /// Invalidates cache by custom criteria
    /// </summary>
    Task InvalidateByCriteriaAsync(Func<string, bool> criteria);
    
    /// <summary>
    /// Adds a dependency between cache keys
    /// </summary>
    Task AddDependencyAsync(string key, string dependentKey);
    
    /// <summary>
    /// Removes a dependency between cache keys
    /// </summary>
    Task RemoveDependencyAsync(string key, string dependentKey);
    
    /// <summary>
    /// Gets all dependencies for a key
    /// </summary>
    Task<IEnumerable<string>> GetDependenciesAsync(string key);
    
    /// <summary>
    /// Invalidates a key and all its dependencies
    /// </summary>
    Task InvalidateWithDependenciesAsync(string key);
    
    /// <summary>
    /// Registers a cache invalidation callback
    /// </summary>
    Task<IDisposable> RegisterInvalidationCallbackAsync(Func<string, Task> callback);
    
    /// <summary>
    /// Gets invalidation statistics
    /// </summary>
    Task<InvalidationStatistics> GetInvalidationStatisticsAsync();
}

/// <summary>
/// Invalidation statistics
/// </summary>
public class InvalidationStatistics
{
    public long TotalInvalidations { get; set; }
    public long InvalidationsByPattern { get; set; }
    public long InvalidationsByTag { get; set; }
    public long InvalidationsByPrefix { get; set; }
    public long InvalidationsBySuffix { get; set; }
    public long InvalidationsByCriteria { get; set; }
    public long InvalidationsWithDependencies { get; set; }
    public long TotalDependencies { get; set; }
    public Dictionary<string, long> InvalidationsByType { get; set; } = new();
    public DateTime LastInvalidation { get; set; }
}

/// <summary>
/// Cache dependency information
/// </summary>
public class CacheDependency
{
    public string Key { get; set; } = string.Empty;
    public string DependentKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DependencyType Type { get; set; } = DependencyType.Strong;
    public TimeSpan? Timeout { get; set; }
}

/// <summary>
/// Dependency type
/// </summary>
public enum DependencyType
{
    Strong,     // Key and dependent key are invalidated together
    Weak,       // Only dependent key is invalidated when key changes
    Conditional // Custom invalidation logic
}
