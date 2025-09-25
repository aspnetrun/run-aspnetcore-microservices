using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.EventSourcing;

/// <summary>
/// Extension methods for event sourcing services
/// </summary>
public static class EventSourcingExtensions
{
    /// <summary>
    /// Adds event sourcing services to the service collection
    /// </summary>
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EventStoreOptions>? configureEventStore = null,
        Action<SnapshotOptions>? configureSnapshots = null)
    {
        // Configure event store options
        var eventStoreOptions = new EventStoreOptions();
        configureEventStore?.Invoke(eventStoreOptions);
        
        services.Configure<EventStoreOptions>(config =>
        {
            config.ConnectionString = eventStoreOptions.ConnectionString;
            config.DatabaseName = eventStoreOptions.DatabaseName;
            config.CollectionName = eventStoreOptions.CollectionName;
            config.EnableCompression = eventStoreOptions.EnableCompression;
            config.EnableEncryption = eventStoreOptions.EnableEncryption;
            config.EncryptionKey = eventStoreOptions.EncryptionKey;
            config.MaxBatchSize = eventStoreOptions.MaxBatchSize;
            config.CommandTimeout = eventStoreOptions.CommandTimeout;
            config.EnableEventSourcing = eventStoreOptions.EnableEventSourcing;
            config.EnableSnapshots = eventStoreOptions.EnableSnapshots;
            config.SnapshotFrequency = eventStoreOptions.SnapshotFrequency;
        });

        // Configure snapshot options
        var snapshotOptions = new SnapshotOptions();
        configureSnapshots?.Invoke(snapshotOptions);
        
        services.Configure<SnapshotOptions>(config =>
        {
            config.EnableSnapshots = snapshotOptions.EnableSnapshots;
            config.SnapshotFrequency = snapshotOptions.SnapshotFrequency;
            config.MaxSnapshotsPerAggregate = snapshotOptions.MaxSnapshotsPerAggregate;
            config.SnapshotRetentionPeriod = snapshotOptions.SnapshotRetentionPeriod;
            config.CompressSnapshots = snapshotOptions.CompressSnapshots;
            config.EncryptSnapshots = snapshotOptions.EncryptSnapshots;
            config.EncryptionKey = snapshotOptions.EncryptionKey;
            config.StorageProvider = snapshotOptions.StorageProvider;
            config.StoragePath = snapshotOptions.StoragePath;
        });

        // Register services
        services.AddScoped<IEventStore, InMemoryEventStore>(); // Default implementation
        services.AddScoped<ISnapshotService, InMemorySnapshotService>(); // Default implementation
        
        // Register event handlers
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        
        return services;
    }

    /// <summary>
    /// Adds event sourcing with custom event store
    /// </summary>
    public static IServiceCollection AddEventSourcing<TEventStore>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EventStoreOptions>? configureEventStore = null,
        Action<SnapshotOptions>? configureSnapshots = null)
        where TEventStore : class, IEventStore
    {
        services.AddEventSourcing(configuration, configureEventStore, configureSnapshots);
        services.AddScoped<IEventStore, TEventStore>();
        return services;
    }

    /// <summary>
    /// Adds event sourcing with custom snapshot service
    /// </summary>
    public static IServiceCollection AddEventSourcing<TEventStore, TSnapshotService>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EventStoreOptions>? configureEventStore = null,
        Action<SnapshotOptions>? configureSnapshots = null)
        where TEventStore : class, IEventStore
        where TSnapshotService : class, ISnapshotService
    {
        services.AddEventSourcing<TEventStore>(configuration, configureEventStore, configureSnapshots);
        services.AddScoped<ISnapshotService, TSnapshotService>();
        return services;
    }
}

/// <summary>
/// Event dispatcher for handling events
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches an event to all handlers
    /// </summary>
    Task DispatchAsync(IDomainEvent @event);
    
    /// <summary>
    /// Registers an event handler
    /// </summary>
    void RegisterHandler<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
    
    /// <summary>
    /// Unregisters an event handler
    /// </summary>
    void UnregisterHandler<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
}

/// <summary>
/// Default event dispatcher implementation
/// </summary>
public class EventDispatcher : IEventDispatcher
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(ILogger<EventDispatcher> logger)
    {
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent @event)
    {
        var eventType = @event.GetType();
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            var tasks = handlers.Select(handler => 
            {
                try
                {
                    return (Task)handler.DynamicInvoke(@event)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching event {EventType}", eventType.Name);
                    return Task.CompletedTask;
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    public void RegisterHandler<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }
        
        _handlers[eventType].Add(handler);
    }

    public void UnregisterHandler<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
}

/// <summary>
/// In-memory event store for development/testing
/// </summary>
public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, List<EventEnvelope>> _streams = new();
    private readonly List<EventEnvelope> _allEvents = new();
    private readonly List<Func<EventEnvelope, Task>> _subscribers = new();
    private readonly ILogger<InMemoryEventStore> _logger;

    public InMemoryEventStore(ILogger<InMemoryEventStore> logger)
    {
        _logger = logger;
    }

    public async Task<long> AppendToStreamAsync(string streamId, IEnumerable<IDomainEvent> events, long expectedVersion = -1)
    {
        if (!_streams.ContainsKey(streamId))
        {
            _streams[streamId] = new List<EventEnvelope>();
        }

        var stream = _streams[streamId];
        var currentVersion = stream.Count > 0 ? stream.Max(e => e.StreamVersion) : -1;

        if (expectedVersion != -1 && currentVersion != expectedVersion)
        {
            throw new InvalidOperationException($"Expected version {expectedVersion}, but current version is {currentVersion}");
        }

        var newVersion = currentVersion + 1;
        var eventList = events.ToList();

        foreach (var @event in eventList)
        {
            var envelope = new EventEnvelope
            {
                Id = Guid.NewGuid(),
                StreamId = streamId,
                StreamVersion = newVersion++,
                Event = @event,
                EventType = @event.GetType().Name,
                EventData = System.Text.Json.JsonSerializer.Serialize(@event),
                Metadata = new EventMetadata
                {
                    Source = "InMemoryEventStore"
                }
            };

            stream.Add(envelope);
            _allEvents.Add(envelope);
        }

        // Notify subscribers
        foreach (var subscriber in _subscribers)
        {
            try
            {
                await subscriber(new EventEnvelope()); // Simplified for demo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying subscriber");
            }
        }

        return newVersion - 1;
    }

    public Task<IEnumerable<EventEnvelope>> GetEventsAsync(string streamId, long fromVersion = 0, long toVersion = long.MaxValue)
    {
        if (_streams.TryGetValue(streamId, out var stream))
        {
            var events = stream.Where(e => e.StreamVersion >= fromVersion && e.StreamVersion <= toVersion);
            return Task.FromResult(events);
        }

        return Task.FromResult(Enumerable.Empty<EventEnvelope>());
    }

    public Task<long> GetStreamVersionAsync(string streamId)
    {
        if (_streams.TryGetValue(streamId, out var stream))
        {
            return Task.FromResult(stream.Count > 0 ? stream.Max(e => e.StreamVersion) : -1);
        }

        return Task.FromResult(-1L);
    }

    public Task<IEnumerable<EventEnvelope>> GetEventsByTypeAsync(string eventType, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var events = _allEvents.Where(e => e.EventType == eventType);
        
        if (fromDate.HasValue)
            events = events.Where(e => e.CreatedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            events = events.Where(e => e.CreatedAt <= toDate.Value);

        return Task.FromResult(events);
    }

    public Task<IEnumerable<EventEnvelope>> GetEventsByAggregateTypeAsync(string aggregateType, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var events = _allEvents.Where(e => e.Event.AggregateType == aggregateType);
        
        if (fromDate.HasValue)
            events = events.Where(e => e.CreatedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            events = events.Where(e => e.CreatedAt <= toDate.Value);

        return Task.FromResult(events);
    }

    public Task<IEnumerable<EventEnvelope>> GetEventsByCorrelationIdAsync(string correlationId)
    {
        var events = _allEvents.Where(e => e.Event.CorrelationId == correlationId);
        return Task.FromResult(events);
    }

    public Task<IEnumerable<EventEnvelope>> GetEventsByUserIdAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var events = _allEvents.Where(e => e.Event.UserId == userId);
        
        if (fromDate.HasValue)
            events = events.Where(e => e.CreatedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            events = events.Where(e => e.CreatedAt <= toDate.Value);

        return Task.FromResult(events);
    }

    public Task<IEnumerable<EventEnvelope>> GetEventsByTenantIdAsync(string tenantId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var events = _allEvents.Where(e => e.Metadata.TenantId == tenantId);
        
        if (fromDate.HasValue)
            events = events.Where(e => e.CreatedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            events = events.Where(e => e.CreatedAt <= toDate.Value);

        return Task.FromResult(events);
    }

    public Task<bool> DeleteStreamAsync(string streamId)
    {
        var removed = _streams.Remove(streamId);
        return Task.FromResult(removed);
    }

    public Task<bool> StreamExistsAsync(string streamId)
    {
        return Task.FromResult(_streams.ContainsKey(streamId));
    }

    public Task<IEnumerable<string>> GetAllStreamIdsAsync()
    {
        return Task.FromResult(_streams.Keys.AsEnumerable());
    }

    public Task<StreamMetadata?> GetStreamMetadataAsync(string streamId)
    {
        if (_streams.TryGetValue(streamId, out var stream))
        {
            var metadata = new StreamMetadata
            {
                StreamId = streamId,
                LastVersion = stream.Count > 0 ? stream.Max(e => e.StreamVersion) : -1,
                LastUpdated = stream.Count > 0 ? stream.Max(e => e.CreatedAt) : DateTime.UtcNow,
                LastEventType = stream.Count > 0 ? stream.Last().EventType : null
            };

            return Task.FromResult<StreamMetadata?>(metadata);
        }

        return Task.FromResult<StreamMetadata?>(null);
    }

    public Task SetStreamMetadataAsync(string streamId, StreamMetadata metadata)
    {
        // In-memory implementation doesn't persist metadata
        return Task.CompletedTask;
    }

    public Task<IDisposable> SubscribeAsync(Func<EventEnvelope, Task> handler, string? eventType = null)
    {
        _subscribers.Add(handler);
        
        var disposable = new EventSubscription(() => _subscribers.Remove(handler));
        return Task.FromResult<IDisposable>(disposable);
    }

    public Task<IDisposable> SubscribeToStreamAsync(string streamId, Func<EventEnvelope, Task> handler)
    {
        // Simplified implementation
        return SubscribeAsync(handler);
    }
}

/// <summary>
/// Event subscription for cleanup
/// </summary>
public class EventSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private bool _disposed;

    public EventSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _unsubscribe();
            _disposed = true;
        }
    }
}

/// <summary>
/// In-memory snapshot service for development/testing
/// </summary>
public class InMemorySnapshotService : ISnapshotService
{
    private readonly Dictionary<string, object> _snapshots = new();
    private readonly Dictionary<string, long> _versions = new();
    private readonly Dictionary<string, List<SnapshotInfo>> _snapshotHistory = new();

    public Task<object?> GetSnapshotAsync(string aggregateId, string aggregateType)
    {
        var key = $"{aggregateType}:{aggregateId}";
        _snapshots.TryGetValue(key, out var snapshot);
        return Task.FromResult(snapshot);
    }

    public Task SaveSnapshotAsync(string aggregateId, string aggregateType, object snapshot, long version)
    {
        var key = $"{aggregateType}:{aggregateId}";
        _snapshots[key] = snapshot;
        _versions[key] = version;

        if (!_snapshotHistory.ContainsKey(key))
        {
            _snapshotHistory[key] = new List<SnapshotInfo>();
        }

        var snapshotInfo = new SnapshotInfo
        {
            Id = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            Version = version,
            CreatedAt = DateTime.UtcNow,
            SnapshotData = System.Text.Json.JsonSerializer.Serialize(snapshot),
            SizeInBytes = System.Text.Json.JsonSerializer.Serialize(snapshot).Length
        };

        _snapshotHistory[key].Add(snapshotInfo);

        return Task.CompletedTask;
    }

    public Task<long> GetLatestSnapshotVersionAsync(string aggregateId, string aggregateType)
    {
        var key = $"{aggregateType}:{aggregateId}";
        _versions.TryGetValue(key, out var version);
        return Task.FromResult(version);
    }

    public Task<IEnumerable<SnapshotInfo>> GetSnapshotsAsync(string aggregateId, string aggregateType)
    {
        var key = $"{aggregateType}:{aggregateId}";
        if (_snapshotHistory.TryGetValue(key, out var snapshots))
        {
            return Task.FromResult(snapshots.AsEnumerable());
        }

        return Task.FromResult(Enumerable.Empty<SnapshotInfo>());
    }

    public Task DeleteOldSnapshotsAsync(string aggregateId, string aggregateType, long keepVersion)
    {
        var key = $"{aggregateType}:{aggregateId}";
        if (_snapshotHistory.TryGetValue(key, out var snapshots))
        {
            var toRemove = snapshots.Where(s => s.Version < keepVersion).ToList();
            foreach (var snapshot in toRemove)
            {
                snapshots.Remove(snapshot);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> ShouldCreateSnapshotAsync(string aggregateId, string aggregateType, long currentVersion)
    {
        var key = $"{aggregateType}:{aggregateId}";
        if (_versions.TryGetValue(key, out var lastSnapshotVersion))
        {
            return Task.FromResult(currentVersion - lastSnapshotVersion >= 100); // Every 100 events
        }

        return Task.FromResult(currentVersion >= 100);
    }

    public Task<SnapshotStatistics> GetSnapshotStatisticsAsync()
    {
        var stats = new SnapshotStatistics
        {
            TotalSnapshots = _snapshots.Count,
            TotalSizeInBytes = _snapshotHistory.Values.SelectMany(s => s).Sum(s => s.SizeInBytes),
            SnapshotsByAggregateType = _snapshotHistory.Count,
            OldestSnapshot = _snapshotHistory.Values.SelectMany(s => s).Min(s => s.CreatedAt),
            NewestSnapshot = _snapshotHistory.Values.SelectMany(s => s).Max(s => s.CreatedAt)
        };

        foreach (var kvp in _snapshotHistory)
        {
            var aggregateType = kvp.Key.Split(':')[0];
            var count = kvp.Value.Count;
            var size = kvp.Value.Sum(s => s.SizeInBytes);

            stats.SnapshotsByType[aggregateType] = count;
            stats.SizeByType[aggregateType] = size;
        }

        return Task.FromResult(stats);
    }
}
