namespace BuildingBlocks.EventSourcing;

/// <summary>
/// Interface for event store operations
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends events to a stream
    /// </summary>
    Task<long> AppendToStreamAsync(string streamId, IEnumerable<IDomainEvent> events, long expectedVersion = -1);
    
    /// <summary>
    /// Gets all events from a stream
    /// </summary>
    Task<IEnumerable<EventEnvelope>> GetEventsAsync(string streamId, long fromVersion = 0, long toVersion = long.MaxValue);
    
    /// <summary>
    /// Gets the current version of a stream
    /// </summary>
    Task<long> GetStreamVersionAsync(string streamId);
    
    /// <summary>
    /// Gets events by type across all streams
    /// </summary>
    Task<IEnumerable<EventEnvelope>> GetEventsByTypeAsync(string eventType, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Gets events by aggregate type
    /// </summary>
    Task<IEnumerable<EventEnvelope>> GetEventsByAggregateTypeAsync(string aggregateType, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Gets events by correlation ID
    /// </summary>
    Task<IEnumerable<EventEnvelope>> GetEventsByCorrelationIdAsync(string correlationId);
    
    /// <summary>
    /// Gets events by user ID
    /// </summary>
    Task<IEnumerable<EventEnvelope>> GetEventsByUserIdAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Gets events by tenant ID
    /// </summary>
    Task<IEnumerable<EventEnvelope>> GetEventsByTenantIdAsync(string tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Deletes a stream
    /// </summary>
    Task<bool> DeleteStreamAsync(string streamId);
    
    /// <summary>
    /// Checks if a stream exists
    /// </summary>
    Task<bool> StreamExistsAsync(string streamId);
    
    /// <summary>
    /// Gets all stream IDs
    /// </summary>
    Task<IEnumerable<string>> GetAllStreamIdsAsync();
    
    /// <summary>
    /// Gets stream metadata
    /// </summary>
    Task<StreamMetadata?> GetStreamMetadataAsync(string streamId);
    
    /// <summary>
    /// Sets stream metadata
    /// </summary>
    Task SetStreamMetadataAsync(string streamId, StreamMetadata metadata);
    
    /// <summary>
    /// Subscribes to events
    /// </summary>
    Task<IDisposable> SubscribeAsync(Func<EventEnvelope, Task> handler, string? eventType = null);
    
    /// <summary>
    /// Subscribes to events from a specific stream
    /// </summary>
    Task<IDisposable> SubscribeToStreamAsync(string streamId, Func<EventEnvelope, Task> handler);
}

/// <summary>
/// Stream metadata
/// </summary>
public class StreamMetadata
{
    public string StreamId { get; set; } = string.Empty;
    public long LastVersion { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? LastEventType { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Event store options
/// </summary>
public class EventStoreOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "EventStore";
    public string CollectionName { get; set; } = "Events";
    public bool EnableCompression { get; set; } = true;
    public bool EnableEncryption { get; set; } = false;
    public string? EncryptionKey { get; set; }
    public int MaxBatchSize { get; set; } = 1000;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableEventSourcing { get; set; } = true;
    public bool EnableSnapshots { get; set; } = true;
    public int SnapshotFrequency { get; set; } = 100; // Every 100 events
}
