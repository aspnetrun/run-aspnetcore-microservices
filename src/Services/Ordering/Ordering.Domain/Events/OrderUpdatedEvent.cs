namespace Ordering.Domain.Events;

public record OrderUpdatedEvent(Order order) : IDomainEvent;
