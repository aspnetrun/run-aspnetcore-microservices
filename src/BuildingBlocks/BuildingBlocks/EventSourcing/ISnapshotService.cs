namespace BuildingBlocks.EventSourcing;

/// <summary>
/// Interface for snapshot services
/// </summary>
public interface ISnapshotService
{
    /// <summary>
    /// Gets a snapshot for an aggregate
    /// </summary>
    Task<object?> GetSnapshotAsync(string aggregateId, string aggregateType);
    
    /// <summary>
    /// Saves a snapshot for an aggregate
    /// </summary>
    Task SaveSnapshotAsync(string aggregateId, string aggregateType, object snapshot, long version);
    
    /// <summary>
    /// Gets the latest snapshot version for an aggregate
    /// </summary>
    Task<long> GetLatestSnapshotVersionAsync(string aggregateId, string aggregateType);
    
    /// <summary>
    /// Gets all snapshots for an aggregate
    /// </summary>
    Task<IEnumerable<SnapshotInfo>> GetSnapshotsAsync(string aggregateId, string aggregateType);
    
    /// <summary>
    /// Deletes old snapshots
    /// </summary>
    Task DeleteOldSnapshotsAsync(string aggregateId, string aggregateType, long keepVersion);
    
    /// <summary>
    /// Checks if snapshot should be created
    /// </summary>
    Task<bool> ShouldCreateSnapshotAsync(string aggregateId, string aggregateType, long currentVersion);
    
    /// <summary>
    /// Gets snapshot statistics
    /// </summary>
    Task<SnapshotStatistics> GetSnapshotStatisticsAsync();
}

/// <summary>
/// Snapshot information
/// </summary>
public class SnapshotInfo
{
    public string Id { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SnapshotData { get; set; } = string.Empty; // Serialized snapshot
    public long SizeInBytes { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Snapshot statistics
/// </summary>
public class SnapshotStatistics
{
    public int TotalSnapshots { get; set; }
    public long TotalSizeInBytes { get; set; }
    public int SnapshotsByAggregateType { get; set; }
    public DateTime OldestSnapshot { get; set; }
    public DateTime NewestSnapshot { get; set; }
    public Dictionary<string, int> SnapshotsByType { get; set; } = new();
    public Dictionary<string, long> SizeByType { get; set; } = new();
}

/// <summary>
/// Snapshot options
/// </summary>
public class SnapshotOptions
{
    public bool EnableSnapshots { get; set; } = true;
    public int SnapshotFrequency { get; set; } = 100; // Every 100 events
    public int MaxSnapshotsPerAggregate { get; set; } = 10;
    public TimeSpan SnapshotRetentionPeriod { get; set; } = TimeSpan.FromDays(90);
    public bool CompressSnapshots { get; set; } = true;
    public bool EncryptSnapshots { get; set; } = false;
    public string? EncryptionKey { get; set; }
    public string StorageProvider { get; set; } = "FileSystem"; // FileSystem, Database, BlobStorage
    public string StoragePath { get; set; } = "Snapshots";
}
