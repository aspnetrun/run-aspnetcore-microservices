using BuildingBlocks.EventSourcing;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Examples;

/// <summary>
/// Example domain events for a product
/// </summary>
public class ProductCreatedEvent : DomainEvent
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    public ProductCreatedEvent(string aggregateId, string name, decimal price, string description, string category)
        : base(aggregateId, 0)
    {
        Name = name;
        Price = price;
        Description = description;
        Category = category;
    }
}

public class ProductUpdatedEvent : DomainEvent
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    public ProductUpdatedEvent(string aggregateId, long version, string name, decimal price, string description, string category)
        : base(aggregateId, version)
    {
        Name = name;
        Price = price;
        Description = description;
        Category = category;
    }
}

public class ProductPriceChangedEvent : DomainEvent
{
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    
    public ProductPriceChangedEvent(string aggregateId, long version, decimal oldPrice, decimal newPrice, string reason)
        : base(aggregateId, version)
    {
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Reason = reason;
    }
}

public class ProductDeletedEvent : DomainEvent
{
    public string DeletedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    
    public ProductDeletedEvent(string aggregateId, long version, string deletedBy, string reason)
        : base(aggregateId, version)
    {
        DeletedBy = deletedBy;
        Reason = reason;
    }
}

/// <summary>
/// Example aggregate for a product
/// </summary>
public class Product : Aggregate
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
    public string? DeletionReason { get; private set; }

    public Product() : base() { }

    public Product(string id, string name, decimal price, string description, string category)
        : base(id)
    {
        RaiseEvent(new ProductCreatedEvent(id, name, price, description, category));
    }

    public void Update(string name, decimal price, string description, string category)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update a deleted product");

        var oldPrice = Price;
        
        RaiseEvent(new ProductUpdatedEvent(Id, Version, name, price, description, category));
        
        if (oldPrice != price)
        {
            RaiseEvent(new ProductPriceChangedEvent(Id, Version, oldPrice, price, "Product updated"));
        }
    }

    public void ChangePrice(decimal newPrice, string reason)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot change price of a deleted product");

        var oldPrice = Price;
        RaiseEvent(new ProductPriceChangedEvent(Id, Version, oldPrice, newPrice, reason));
    }

    public void Delete(string deletedBy, string reason)
    {
        if (!IsActive)
            throw new InvalidOperationException("Product is already deleted");

        RaiseEvent(new ProductDeletedEvent(Id, Version, deletedBy, reason));
    }

    protected override void RegisterEventHandlers()
    {
        RegisterEventHandler<ProductCreatedEvent>(Handle);
        RegisterEventHandler<ProductUpdatedEvent>(Handle);
        RegisterEventHandler<ProductPriceChangedEvent>(Handle);
        RegisterEventHandler<ProductDeletedEvent>(Handle);
    }

    private void Handle(ProductCreatedEvent @event)
    {
        Name = @event.Name;
        Price = @event.Price;
        Description = @event.Description;
        Category = @event.Category;
        IsActive = true;
    }

    private void Handle(ProductUpdatedEvent @event)
    {
        Name = @event.Name;
        Price = @event.Price;
        Description = @event.Description;
        Category = @event.Category;
    }

    private void Handle(ProductPriceChangedEvent @event)
    {
        Price = @event.NewPrice;
    }

    private void Handle(ProductDeletedEvent @event)
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = @event.DeletedBy;
        DeletionReason = @event.Reason;
    }

    public override object GetSnapshot()
    {
        return new ProductSnapshot
        {
            Id = Id,
            Name = Name,
            Price = Price,
            Description = Description,
            Category = Category,
            IsActive = IsActive,
            Version = Version,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }

    public override void LoadFromSnapshot(object snapshot)
    {
        if (snapshot is ProductSnapshot productSnapshot)
        {
            Id = productSnapshot.Id;
            Name = productSnapshot.Name;
            Price = productSnapshot.Price;
            Description = productSnapshot.Description;
            Category = productSnapshot.Category;
            IsActive = productSnapshot.IsActive;
            Version = productSnapshot.Version;
            CreatedAt = productSnapshot.CreatedAt;
            LastModifiedAt = productSnapshot.LastModifiedAt;
        }
    }
}

/// <summary>
/// Product snapshot for performance optimization
/// </summary>
public class ProductSnapshot
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public long Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

/// <summary>
/// Example controller showing event sourcing building blocks usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventSourcingExampleController : ControllerBase
{
    private readonly IEventStore _eventStore;
    private readonly ISnapshotService _snapshotService;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IAggregateRepository<Product> _productRepository;

    public EventSourcingExampleController(
        IEventStore eventStore,
        ISnapshotService snapshotService,
        IEventDispatcher eventDispatcher,
        IAggregateRepository<Product> productRepository)
    {
        _eventStore = eventStore;
        _snapshotService = snapshotService;
        _eventDispatcher = eventDispatcher;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = new Product(
            Guid.NewGuid().ToString(),
            request.Name,
            request.Price,
            request.Description,
            request.Category
        );

        await _productRepository.SaveAsync(product);

        // Dispatch events
        foreach (var @event in product.GetUncommittedEvents())
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        return Ok(new
        {
            id = product.Id,
            name = product.Name,
            price = product.Price,
            version = product.Version,
            message = "Product created successfully"
        });
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound($"Product {id} not found");

        return Ok(new
        {
            id = product.Id,
            name = product.Name,
            price = product.Price,
            description = product.Description,
            category = product.Category,
            isActive = product.IsActive,
            version = product.Version,
            createdAt = product.CreatedAt,
            lastModifiedAt = product.LastModifiedAt
        });
    }

    /// <summary>
    /// Update a product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound($"Product {id} not found");

        product.Update(request.Name, request.Price, request.Description, request.Category);
        
        await _productRepository.SaveAsync(product);

        // Dispatch events
        foreach (var @event in product.GetUncommittedEvents())
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        return Ok(new
        {
            id = product.Id,
            name = product.Name,
            price = product.Price,
            version = product.Version,
            message = "Product updated successfully"
        });
    }

    /// <summary>
    /// Change product price
    /// </summary>
    [HttpPatch("{id}/price")]
    public async Task<IActionResult> ChangePrice(string id, [FromBody] ChangePriceRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound($"Product {id} not found");

        product.ChangePrice(request.NewPrice, request.Reason);
        
        await _productRepository.SaveAsync(product);

        // Dispatch events
        foreach (var @event in product.GetUncommittedEvents())
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        return Ok(new
        {
            id = product.Id,
            oldPrice = product.Price,
            newPrice = request.NewPrice,
            version = product.Version,
            message = "Price changed successfully"
        });
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id, [FromBody] DeleteProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        
        if (product == null)
            return NotFound($"Product {id} not found");

        product.Delete(request.DeletedBy, request.Reason);
        
        await _productRepository.SaveAsync(product);

        // Dispatch events
        foreach (var @event in product.GetUncommittedEvents())
        {
            await _eventDispatcher.DispatchAsync(@event);
        }

        return Ok(new
        {
            id = product.Id,
            deletedAt = product.DeletedAt,
            deletedBy = product.DeletedBy,
            reason = product.DeletionReason,
            message = "Product deleted successfully"
        });
    }

    /// <summary>
    /// Get product event history
    /// </summary>
    [HttpGet("{id}/events")]
    public async Task<IActionResult> GetProductEvents(string id, [FromQuery] long fromVersion = 0)
    {
        var events = await _eventStore.GetEventsAsync($"Product:{id}", fromVersion);
        
        return Ok(new
        {
            productId = id,
            events = events.Select(e => new
            {
                id = e.Id,
                type = e.EventType,
                version = e.StreamVersion,
                occurredOn = e.Event.OccurredOn,
                data = e.EventData
            })
        });
    }

    /// <summary>
    /// Get product snapshot
    /// </summary>
    [HttpGet("{id}/snapshot")]
    public async Task<IActionResult> GetProductSnapshot(string id)
    {
        var snapshot = await _snapshotService.GetSnapshotAsync(id, "Product");
        
        if (snapshot == null)
            return NotFound($"No snapshot found for product {id}");

        return Ok(snapshot);
    }

    /// <summary>
    /// Get events by type
    /// </summary>
    [HttpGet("events/{eventType}")]
    public async Task<IActionResult> GetEventsByType(string eventType, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var events = await _eventStore.GetEventsByTypeAsync(eventType, fromDate, toDate);
        
        return Ok(new
        {
            eventType,
            count = events.Count(),
            events = events.Select(e => new
            {
                id = e.Id,
                aggregateId = e.Event.AggregateId,
                version = e.StreamVersion,
                occurredOn = e.Event.OccurredOn,
                data = e.EventData
            })
        });
    }

    /// <summary>
    /// Get snapshot statistics
    /// </summary>
    [HttpGet("snapshots/stats")]
    public async Task<IActionResult> GetSnapshotStatistics()
    {
        var stats = await _snapshotService.GetSnapshotStatisticsAsync();
        return Ok(stats);
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class ChangePriceRequest
{
    public decimal NewPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DeleteProductRequest
{
    public string DeletedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
