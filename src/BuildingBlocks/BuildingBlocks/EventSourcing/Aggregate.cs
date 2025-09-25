namespace BuildingBlocks.EventSourcing;

/// <summary>
/// Base class for event-sourced aggregates
/// </summary>
public abstract class Aggregate : IAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    private readonly Dictionary<Type, Action<IDomainEvent>> _eventHandlers = new();
    
    public string Id { get; protected set; } = string.Empty;
    public long Version { get; protected set; } = -1;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; protected set; } = DateTime.UtcNow;
    
    protected Aggregate()
    {
        RegisterEventHandlers();
    }
    
    protected Aggregate(string id) : this()
    {
        Id = id;
    }
    
    /// <summary>
    /// Gets uncommitted events
    /// </summary>
    public IEnumerable<IDomainEvent> GetUncommittedEvents()
    {
        return _uncommittedEvents.AsReadOnly();
    }
    
    /// <summary>
    /// Marks events as committed
    /// </summary>
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }
    
    /// <summary>
    /// Loads aggregate from events
    /// </summary>
    public void LoadFromEvents(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            ApplyEvent(@event);
            Version = @event.Version;
        }
    }
    
    /// <summary>
    /// Applies an event to the aggregate
    /// </summary>
    protected void ApplyEvent(IDomainEvent @event)
    {
        if (_eventHandlers.TryGetValue(@event.GetType(), out var handler))
        {
            handler(@event);
        }
        
        LastModifiedAt = @event.OccurredOn;
    }
    
    /// <summary>
    /// Raises a new event
    /// </summary>
    protected void RaiseEvent<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        @event.Version = Version + 1;
        @event.AggregateId = Id;
        @event.AggregateType = GetType().Name;
        
        ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
        
        Version = @event.Version;
    }
    
    /// <summary>
    /// Registers event handlers
    /// </summary>
    protected abstract void RegisterEventHandlers();
    
    /// <summary>
    /// Registers an event handler
    /// </summary>
    protected void RegisterEventHandler<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
    {
        _eventHandlers[typeof(TEvent)] = @event => handler((TEvent)@event);
    }
    
    /// <summary>
    /// Validates aggregate state
    /// </summary>
    protected virtual void ValidateState()
    {
        // Override in derived classes to add validation logic
    }
    
    /// <summary>
    /// Gets aggregate snapshot
    /// </summary>
    public virtual object GetSnapshot()
    {
        return this;
    }
    
    /// <summary>
    /// Loads aggregate from snapshot
    /// </summary>
    public virtual void LoadFromSnapshot(object snapshot)
    {
        // Override in derived classes to implement snapshot loading
    }
}

/// <summary>
/// Interface for aggregates
/// </summary>
public interface IAggregate
{
    string Id { get; }
    long Version { get; }
    DateTime CreatedAt { get; }
    DateTime LastModifiedAt { get; }
    IEnumerable<IDomainEvent> GetUncommittedEvents();
    void MarkEventsAsCommitted();
    void LoadFromEvents(IEnumerable<IDomainEvent> events);
    object GetSnapshot();
    void LoadFromSnapshot(object snapshot);
}

/// <summary>
/// Repository interface for aggregates
/// </summary>
public interface IAggregateRepository<TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Gets an aggregate by ID
    /// </summary>
    Task<TAggregate?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets an aggregate by ID with specific version
    /// </summary>
    Task<TAggregate?> GetByIdAsync(string id, long version);
    
    /// <summary>
    /// Saves an aggregate
    /// </summary>
    Task SaveAsync(TAggregate aggregate);
    
    /// <summary>
    /// Gets aggregate version
    /// </summary>
    Task<long> GetVersionAsync(string id);
    
    /// <summary>
    /// Checks if aggregate exists
    /// </summary>
    Task<bool> ExistsAsync(string id);
    
    /// <summary>
    /// Deletes an aggregate
    /// </summary>
    Task<bool> DeleteAsync(string id);
}
