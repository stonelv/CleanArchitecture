namespace MinimalClean.Architecture.Web.Domain.OrderAggregate.Events;

public sealed class OrderCreatedEvent(Order order) : DomainEventBase
{
    public Order Order { get; private set; } = order;
}
