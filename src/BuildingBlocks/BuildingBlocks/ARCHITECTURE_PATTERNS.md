# 🏗️ BuildingBlocks Architecture Patterns

## Overview

BuildingBlocks leverage fundamental architecture patterns to create a robust, maintainable, and extensible system. This document explains how these patterns work together to provide enterprise-grade functionality.

## 1. **Abstraction (Interface Segregation)**

### **Core Principle: "Program to an interface, not an implementation"**

BuildingBlocks use **interface segregation** to provide clean, focused contracts:

```csharp
// Instead of one monolithic interface, we have focused ones:
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, CacheOptions? options = null);
    Task<bool> RemoveAsync(string key);
    // ... other cache operations
}

public interface IDistributedCacheService : ICacheService
{
    Task<IDistributedLock> AcquireLockAsync(string key, TimeSpan timeout);
    Task PublishAsync(string channel, object message);
    Task<IDisposable> SubscribeAsync(string channel, Func<string, object, Task> handler);
    // ... distributed-specific operations
}

public interface ICacheInvalidationService
{
    Task InvalidateByTagAsync(string tag);
    Task InvalidateByPatternAsync(string pattern);
    Task InvalidateByPrefixAsync(string prefix);
    // ... invalidation-specific operations
}
```

### **Benefits:**
- **Single Responsibility** - Each interface has one clear purpose
- **Loose Coupling** - Consumers depend only on what they need
- **Testability** - Easy to mock specific behaviors
- **Flexibility** - Can implement only the interfaces you need

## 2. **Inheritance (Template Method Pattern)**

### **Core Principle: "Define the skeleton of an algorithm, letting subclasses override specific steps"**

BuildingBlocks use inheritance to provide **default implementations** while allowing customization:

```csharp
// Base class provides common functionality
public abstract class Aggregate : IAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    private readonly Dictionary<Type, Action<IDomainEvent>> _eventHandlers = new();
    
    // Template method - defines the algorithm
    public void RaiseEvent<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        // Common logic
        @event.AggregateId = Id;
        @event.Version = Version;
        @event.Timestamp = DateTime.UtcNow;
        
        // Hook for subclasses
        ApplyEvent(@event);
        
        // Common logic
        _uncommittedEvents.Add(@event);
    }
    
    // Abstract method - subclasses must implement
    protected abstract void ApplyEvent(IDomainEvent @event);
    
    // Virtual method - subclasses can override
    protected virtual void RegisterEventHandlers()
    {
        // Default implementation
    }
}

// Concrete implementation
public class ProductAggregate : Aggregate
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    
    // Must implement abstract method
    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case ProductCreatedEvent e:
                Name = e.Name;
                Price = e.Price;
                break;
            case ProductPriceChangedEvent e:
                Price = e.NewPrice;
                break;
        }
    }
    
    // Can override virtual method
    protected override void RegisterEventHandlers()
    {
        // Custom event handler registration
    }
}
```

### **Benefits:**
- **Code Reuse** - Common logic in base class
- **Consistency** - All aggregates follow same pattern
- **Extensibility** - Easy to add new aggregate types
- **Maintainability** - Changes in base class affect all subclasses

## 3. **Dependency Injection (IoC Container Pattern)**

### **Core Principle: "Depend on abstractions, not concretions"**

BuildingBlocks use **constructor injection** and **service registration** for loose coupling:

```csharp
// Service registration with fluent API
public static class CachingExtensions
{
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RedisCacheOptions>? configureOptions = null)
    {
        // Register options
        services.Configure<RedisCacheOptions>(config => { /* ... */ });
        
        // Register Redis connection
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });
        
        // Register services
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IDistributedCacheService, RedisCacheService>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        
        return services;
    }
}

// Constructor injection in services
public class RedisCacheService : IDistributedCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly RedisCacheOptions _options;
    
    public RedisCacheService(
        IConnectionMultiplexer redis,           // Injected dependency
        ILogger<RedisCacheService> logger,      // Injected dependency
        IOptions<RedisCacheOptions> options)    // Injected dependency
    {
        _redis = redis;
        _logger = logger;
        _options = options.Value;
    }
}

// Usage in controllers
public class CachingExampleController : ControllerBase
{
    private readonly ICacheService _cacheService;                    // Interface dependency
    private readonly IDistributedCacheService _distributedCache;     // Interface dependency
    private readonly ICacheInvalidationService _invalidationService; // Interface dependency
    
    public CachingExampleController(
        ICacheService cacheService,                    // Injected
        IDistributedCacheService distributedCache,     // Injected
        ICacheInvalidationService invalidationService, // Injected
        ILogger<CachingExampleController> logger)
    {
        _cacheService = cacheService;
        _distributedCache = distributedCache;
        _invalidationService = invalidationService;
    }
}
```

### **Benefits:**
- **Testability** - Easy to inject mocks for testing
- **Flexibility** - Can swap implementations without code changes
- **Loose Coupling** - Dependencies are explicit and manageable
- **Configuration** - Services can be configured differently per environment

## 4. **Strategy Pattern**

### **Core Principle: "Define a family of algorithms, encapsulate each one, and make them interchangeable"**

BuildingBlocks use strategy pattern for **different cache implementations**:

```csharp
// Strategy interface
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, CacheOptions? options = null);
    // ... other methods
}

// Concrete strategies
public class RedisCacheService : ICacheService { /* Redis implementation */ }
public class InMemoryCacheService : ICacheService { /* Memory implementation */ }
public class HybridCacheService : ICacheService { /* Combined implementation */ }

// Strategy selection based on configuration
public static class CachingExtensions
{
    public static IServiceCollection AddHybridCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register multiple strategies
        services.AddRedisCaching(configuration);
        services.AddInMemoryCaching();
        
        // Register the hybrid strategy that combines both
        services.AddScoped<ICacheService, HybridCacheService>();
        
        return services;
    }
}

// Hybrid strategy implementation
public class HybridCacheService : ICacheService
{
    private readonly IDistributedCacheService _redisCache;    // Strategy 1
    private readonly ICacheService _memoryCache;             // Strategy 2
    
    public async Task<T?> GetAsync<T>(string key)
    {
        // Try memory first (fast)
        var memoryValue = await _memoryCache.GetAsync<T>(key);
        if (memoryValue != null) return memoryValue;
        
        // Fall back to Redis (persistent)
        var redisValue = await _redisCache.GetAsync<T>(key);
        if (redisValue != null)
        {
            // Cache in memory for next time
            await _memoryCache.SetAsync(key, redisValue);
            return redisValue;
        }
        
        return default;
    }
}
```

### **Benefits:**
- **Runtime Selection** - Choose strategy based on configuration
- **Easy Extension** - Add new strategies without changing existing code
- **Algorithm Encapsulation** - Each strategy encapsulates its logic
- **Composition** - Strategies can be combined for complex behaviors

## 5. **Decorator Pattern**

### **Core Principle: "Attach additional responsibilities to an object dynamically"**

BuildingBlocks use decorators for **cross-cutting concerns**:

```csharp
// Base interface
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, CacheOptions? options = null);
}

// Concrete implementation
public class RedisCacheService : ICacheService { /* ... */ }

// Decorator for logging
public class LoggingCacheDecorator : ICacheService
{
    private readonly ICacheService _innerCache;
    private readonly ILogger<LoggingCacheDecorator> _logger;
    
    public LoggingCacheDecorator(ICacheService innerCache, ILogger<LoggingCacheDecorator> logger)
    {
        _innerCache = innerCache;
        _logger = logger;
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        _logger.LogDebug("Getting value for key: {Key}", key);
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await _innerCache.GetAsync<T>(key);
            stopwatch.Stop();
            
            _logger.LogDebug("Cache {Result} for key: {Key} in {ElapsedMs}ms", 
                result != null ? "hit" : "miss", key, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error getting value for key: {Key} after {ElapsedMs}ms", 
                key, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

// Decorator for metrics
public class MetricsCacheDecorator : ICacheService
{
    private readonly ICacheService _innerCache;
    private readonly IMetricsCollector _metrics;
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await _innerCache.GetAsync<T>(key);
        stopwatch.Stop();
        
        _metrics.RecordCacheOperation("get", key, result != null, stopwatch.Elapsed);
        return result;
    }
}

// Decorator for retry logic
public class RetryCacheDecorator : ICacheService
{
    private readonly ICacheService _innerCache;
    private readonly IRetryPolicy _retryPolicy;
    
    public async Task<T?> GetAsync<T>(string key)
    {
        return await _retryPolicy.ExecuteAsync(async () => 
            await _innerCache.GetAsync<T>(key));
    }
}

// Registration with decorators
public static class CachingExtensions
{
    public static IServiceCollection AddRedisCachingWithDecorators(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register base service
        services.AddRedisCaching(configuration);
        
        // Decorate with cross-cutting concerns
        services.Decorate<ICacheService, LoggingCacheDecorator>();
        services.Decorate<ICacheService, MetricsCacheDecorator>();
        services.Decorate<ICacheService, RetryCacheDecorator>();
        
        return services;
    }
}
```

### **Benefits:**
- **Single Responsibility** - Each decorator handles one concern
- **Open/Closed Principle** - Open for extension, closed for modification
- **Composable** - Decorators can be combined in any order
- **Testable** - Each decorator can be tested independently

## 6. **Factory Pattern**

### **Core Principle: "Create objects without specifying their exact classes"**

BuildingBlocks use factories for **complex object creation**:

```csharp
// Factory interface
public interface ICacheServiceFactory
{
    ICacheService CreateCacheService(CacheType type, CacheOptions options);
}

// Factory implementation
public class CacheServiceFactory : ICacheServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    
    public ICacheService CreateCacheService(CacheType type, CacheOptions options)
    {
        return type switch
        {
            CacheType.Redis => CreateRedisCache(options),
            CacheType.Memory => CreateMemoryCache(options),
            CacheType.Hybrid => CreateHybridCache(options),
            _ => throw new ArgumentException($"Unknown cache type: {type}")
        };
    }
    
    private ICacheService CreateRedisCache(CacheOptions options)
    {
        var redisOptions = new RedisCacheOptions
        {
            ConnectionString = _configuration.GetConnectionString("Redis"),
            DefaultTimeToLive = options.TimeToLive,
            EnableCompression = options.Compress,
            EnableEncryption = options.Encrypt
        };
        
        var connection = ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        var logger = _serviceProvider.GetRequiredService<ILogger<RedisCacheService>>();
        
        return new RedisCacheService(connection, logger, Options.Create(redisOptions));
    }
}

// Usage
public class CacheController : ControllerBase
{
    private readonly ICacheServiceFactory _cacheFactory;
    
    public async Task<IActionResult> CreateCache([FromBody] CreateCacheRequest request)
    {
        var cacheService = _cacheFactory.CreateCacheService(
            request.Type, 
            request.Options);
        
        // Use the created cache service
        await cacheService.SetAsync("test", "value");
        
        return Ok("Cache created successfully");
    }
}
```

### **Benefits:**
- **Encapsulation** - Complex creation logic is hidden
- **Flexibility** - Easy to add new cache types
- **Configuration** - Creation can be driven by configuration
- **Testing** - Easy to mock factory for testing

## 7. **Observer Pattern (Event-Driven Architecture)**

### **Core Principle: "Define a one-to-many dependency between objects so that when one object changes state, all its dependents are notified"**

BuildingBlocks use observers for **cache invalidation** and **audit logging**:

```csharp
// Subject interface
public interface ICacheInvalidationService
{
    Task<IDisposable> RegisterInvalidationCallbackAsync(Func<string, Task> callback);
    Task InvalidateByTagAsync(string tag);
}

// Observer registration
public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly List<Func<string, Task>> _invalidationCallbacks = new();
    
    public async Task<IDisposable> RegisterInvalidationCallbackAsync(Func<string, Task> callback)
    {
        _invalidationCallbacks.Add(callback);
        return new InvalidationCallbackRegistration(() => _invalidationCallbacks.Remove(callback));
    }
    
    private async Task NotifyInvalidationCallbacksAsync(string key)
    {
        var tasks = _invalidationCallbacks.Select(callback => callback(key));
        await Task.WhenAll(tasks);
    }
}

// Observer implementation
public class AuditService
{
    public async Task OnCacheInvalidation(string key)
    {
        await LogAuditEntryAsync(new AuditEntry
        {
            Action = "CacheInvalidation",
            EntityType = "Cache",
            EntityId = key,
            Timestamp = DateTime.UtcNow
        });
    }
}

// Registration and usage
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddScoped<AuditService>();
    }
    
    public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
    {
        // Register observers
        var invalidationService = serviceProvider.GetRequiredService<ICacheInvalidationService>();
        var auditService = serviceProvider.GetRequiredService<AuditService>();
        
        invalidationService.RegisterInvalidationCallbackAsync(
            auditService.OnCacheInvalidation);
    }
}
```

### **Benefits:**
- **Loose Coupling** - Subjects don't know about observers
- **Extensible** - Easy to add new observers
- **Asynchronous** - Non-blocking notification
- **Composable** - Multiple observers can be registered

## 8. **Repository Pattern**

### **Core Principle: "Abstract the data persistence logic"**

BuildingBlocks use repositories for **tenant management** and **event storage**:

```csharp
// Repository interface
public interface ITenantRepository
{
    Task<TenantInfo?> GetByIdAsync(string id);
    Task<TenantInfo?> GetByDomainAsync(string domain);
    Task<TenantInfo> CreateAsync(TenantInfo tenant);
    Task<TenantInfo> UpdateAsync(TenantInfo tenant);
    Task<bool> DeleteAsync(string id);
}

// In-memory implementation for development
public class InMemoryTenantRepository : ITenantRepository
{
    private readonly Dictionary<string, TenantInfo> _tenants = new();
    private readonly object _lock = new();
    
    public async Task<TenantInfo?> GetByIdAsync(string id)
    {
        await Task.Delay(1); // Simulate async
        lock (_lock)
        {
            return _tenants.TryGetValue(id, out var tenant) ? tenant : null;
        }
    }
    
    public async Task<TenantInfo> CreateAsync(TenantInfo tenant)
    {
        await Task.Delay(1); // Simulate async
        lock (_lock)
        {
            _tenants[tenant.Id] = tenant;
            return tenant;
        }
    }
}

// Redis implementation for production
public class RedisTenantRepository : ITenantRepository
{
    private readonly IDatabase _redis;
    private readonly ILogger<RedisTenantRepository> _logger;
    
    public async Task<TenantInfo?> GetByIdAsync(string id)
    {
        var key = $"tenant:{id}";
        var value = await _redis.StringGetAsync(key);
        
        if (!value.HasValue) return null;
        
        return JsonSerializer.Deserialize<TenantInfo>(value!);
    }
    
    public async Task<TenantInfo> CreateAsync(TenantInfo tenant)
    {
        var key = $"tenant:{tenant.Id}";
        var value = JsonSerializer.Serialize(tenant);
        
        await _redis.StringSetAsync(key, value, TimeSpan.FromDays(365));
        
        return tenant;
    }
}

// Factory for repository selection
public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useRedis = configuration.GetValue<bool>("MultiTenancy:UseRedis", false);
        
        if (useRedis)
        {
            services.AddScoped<ITenantRepository, RedisTenantRepository>();
        }
        else
        {
            services.AddScoped<ITenantRepository, InMemoryTenantRepository>();
        }
        
        return services;
    }
}
```

### **Benefits:**
- **Data Access Abstraction** - Business logic doesn't know about storage
- **Testability** - Easy to mock repositories
- **Flexibility** - Can swap storage implementations
- **Consistency** - Same interface regardless of storage

## 🎯 **Architecture Benefits Summary**

### **1. Maintainability**
- **Single Responsibility** - Each class has one reason to change
- **Open/Closed** - Open for extension, closed for modification
- **Dependency Inversion** - High-level modules don't depend on low-level modules

### **2. Testability**
- **Interface Segregation** - Easy to mock specific behaviors
- **Constructor Injection** - Dependencies can be replaced with test doubles
- **Strategy Pattern** - Different implementations can be tested independently

### **3. Extensibility**
- **Template Method** - Common algorithms with customizable steps
- **Decorator Pattern** - Cross-cutting concerns can be added without modification
- **Factory Pattern** - New types can be added easily

### **4. Performance**
- **Lazy Loading** - Services are created only when needed
- **Connection Pooling** - Efficient resource management
- **Async/Await** - Non-blocking operations throughout

### **5. Security**
- **Interface Segregation** - Services only expose what they need
- **Dependency Injection** - No direct instantiation of concrete classes
- **Strategy Pattern** - Security implementations can be swapped

## 🚀 **Real-World Impact**

These architectural patterns enable BuildingBlocks to:

1. **Scale** - Handle enterprise workloads efficiently
2. **Adapt** - Easy to modify and extend
3. **Integrate** - Work seamlessly with existing systems
4. **Maintain** - Reduce technical debt and maintenance costs
5. **Test** - Comprehensive testing and validation
6. **Deploy** - Consistent deployment across environments

## 📚 **Additional Patterns Used**

### **9. Command Query Responsibility Segregation (CQRS)**
- **Commands** - Write operations that change state
- **Queries** - Read operations that don't change state
- **Handlers** - Business logic for processing commands and queries

### **10. Mediator Pattern**
- **IMediator** - Central message routing
- **Request/Response** - Encapsulated message handling
- **Pipeline** - Cross-cutting concerns in message processing

### **11. Specification Pattern**
- **ISpecification<T>** - Encapsulated business rules
- **Composite** - Complex specifications built from simple ones
- **Repository Integration** - Database query optimization

### **12. Unit of Work Pattern**
- **Transaction Management** - Consistent data changes
- **Repository Coordination** - Multiple repository operations
- **Rollback Support** - Error handling and recovery

## 🔧 **Implementation Guidelines**

### **When to Use Each Pattern:**

1. **Interface Segregation** - When you have clients that only need part of an interface
2. **Template Method** - When you have algorithms with common structure but varying steps
3. **Dependency Injection** - Always for service dependencies
4. **Strategy Pattern** - When you need to switch algorithms at runtime
5. **Decorator Pattern** - When you need to add behavior without inheritance
6. **Factory Pattern** - When object creation is complex or configurable
7. **Observer Pattern** - When you need loose coupling between components
8. **Repository Pattern** - When you need to abstract data access

### **Best Practices:**

1. **Start Simple** - Begin with basic patterns and add complexity as needed
2. **Consistent Naming** - Use clear, descriptive names for interfaces and implementations
3. **Documentation** - Document the purpose and usage of each pattern
4. **Testing** - Write tests that verify pattern behavior
5. **Performance** - Monitor and optimize pattern implementations
6. **Maintenance** - Regularly review and refactor pattern usage

## 🎉 **Conclusion**

BuildingBlocks demonstrate how architectural patterns can be combined to create a robust, maintainable, and extensible system. By understanding and applying these patterns, developers can:

- **Build Better Software** - More maintainable, testable, and extensible
- **Solve Common Problems** - Leverage proven solutions to recurring challenges
- **Improve Team Productivity** - Consistent patterns reduce cognitive load
- **Enable Innovation** - Solid foundation allows focus on business value

The key is not just knowing these patterns, but understanding when and how to apply them effectively. BuildingBlocks provide a practical example of how to implement these patterns in a real-world, enterprise-grade system.

