namespace BuildingBlocks.Caching;

/// <summary>
/// Distributed cache service for multi-instance scenarios
/// </summary>
public interface IDistributedCacheService : ICacheService
{
    /// <summary>
    /// Locks a key for distributed operations
    /// </summary>
    Task<IDistributedLock> AcquireLockAsync(string key, TimeSpan timeout);
    
    /// <summary>
    /// Tries to acquire a lock with a timeout
    /// </summary>
    Task<IDistributedLock?> TryAcquireLockAsync(string key, TimeSpan timeout);
    
    /// <summary>
    /// Publishes a message to a channel
    /// </summary>
    Task PublishAsync(string channel, object message);
    
    /// <summary>
    /// Subscribes to a channel
    /// </summary>
    Task<IDisposable> SubscribeAsync(string channel, Func<string, object, Task> handler);
    
    /// <summary>
    /// Gets the number of subscribers for a channel
    /// </summary>
    Task<long> GetSubscriberCountAsync(string channel);
    
    /// <summary>
    /// Sets a value with distributed locking
    /// </summary>
    Task<bool> SetWithLockAsync<T>(string key, T value, TimeSpan lockTimeout, CacheOptions? options = null);
    
    /// <summary>
    /// Executes an action with distributed locking
    /// </summary>
    Task<T> ExecuteWithLockAsync<T>(string key, TimeSpan lockTimeout, Func<Task<T>> action);
    
    /// <summary>
    /// Gets cache health status
    /// </summary>
    Task<CacheHealthStatus> GetHealthStatusAsync();
    
    /// <summary>
    /// Gets cluster information
    /// </summary>
    Task<ClusterInfo> GetClusterInfoAsync();
}

/// <summary>
/// Distributed lock interface
/// </summary>
public interface IDistributedLock : IDisposable
{
    /// <summary>
    /// Whether the lock is acquired
    /// </summary>
    bool IsAcquired { get; }
    
    /// <summary>
    /// Lock key
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Lock timeout
    /// </summary>
    TimeSpan Timeout { get; }
    
    /// <summary>
    /// When the lock was acquired
    /// </summary>
    DateTime AcquiredAt { get; }
    
    /// <summary>
    /// Extends the lock timeout
    /// </summary>
    Task<bool> ExtendAsync(TimeSpan extension);
    
    /// <summary>
    /// Releases the lock
    /// </summary>
    Task ReleaseAsync();
}

/// <summary>
/// Cache health status
/// </summary>
public class CacheHealthStatus
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public long ActiveConnections { get; set; }
    public long PendingOperations { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Cluster information
/// </summary>
public class ClusterInfo
{
    public string ClusterName { get; set; } = string.Empty;
    public int NodeCount { get; set; }
    public List<NodeInfo> Nodes { get; set; } = new();
    public string PrimaryNode { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public DateTime LastHealthCheck { get; set; }
}

/// <summary>
/// Node information
/// </summary>
public class NodeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsConnected { get; set; }
    public long MemoryUsage { get; set; }
    public long CpuUsage { get; set; }
    public DateTime LastSeen { get; set; }
}
