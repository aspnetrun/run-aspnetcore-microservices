namespace BuildingBlocks.EventSourcing;

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// Version of the aggregate when this event was created
    /// </summary>
    long Version { get; }
    
    /// <summary>
    /// ID of the aggregate that raised this event
    /// </summary>
    string AggregateId { get; }
    
    /// <summary>
    /// Type of the aggregate
    /// </summary>
    string AggregateType { get; }
    
    /// <summary>
    /// User who triggered this event
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    string? CorrelationId { get; }
    
    /// <summary>
    /// Causation ID for event chaining
    /// </summary>
    string? CausationId { get; }
}

/// <summary>
/// Base implementation of domain event
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public long Version { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    
    protected DomainEvent()
    {
        AggregateType = GetType().Name;
    }
    
    protected DomainEvent(string aggregateId, long version)
    {
        AggregateId = aggregateId;
        Version = version;
        AggregateType = GetType().Name;
    }
}

/// <summary>
/// Event metadata for additional context
/// </summary>
public class EventMetadata
{
    public string? TenantId { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new();
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>
/// Event envelope for storing events with metadata
/// </summary>
public class EventEnvelope
{
    public Guid Id { get; set; }
    public string StreamId { get; set; } = string.Empty;
    public long StreamVersion { get; set; }
    public IDomainEvent Event { get; set; } = null!;
    public EventMetadata Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty; // Serialized event data
}
