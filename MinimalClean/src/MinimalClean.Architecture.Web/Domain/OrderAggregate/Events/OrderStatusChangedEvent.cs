namespace MinimalClean.Architecture.Web.Domain.OrderAggregate.Events;

public sealed class OrderStatusChangedEvent(Order order, OrderStatus oldStatus, OrderStatus newStatus) : DomainEventBase
{
    public Order Order { get; private set; } = order;
    public OrderStatus OldStatus { get; private set; } = oldStatus;
    public OrderStatus NewStatus { get; private set; } = newStatus;
}
