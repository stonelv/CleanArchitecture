using Ardalis.GuardClauses;
using MinimalClean.Architecture.Web.Domain.OrderAggregate.Events;

namespace MinimalClean.Architecture.Web.Domain.OrderAggregate.Handlers;

public class OrderStatusChangedLoggingHandler(ILogger<OrderStatusChangedLoggingHandler> logger)
    : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedLoggingHandler> _logger = logger;

    public ValueTask Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        _logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            notification.Order.Id.Value,
            notification.OldStatus,
            notification.NewStatus);

        return ValueTask.CompletedTask;
    }
}
