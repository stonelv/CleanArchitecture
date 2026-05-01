using Ardalis.GuardClauses;
using MinimalClean.Architecture.Web.Domain.OrderAggregate.Events;
using MinimalClean.Architecture.Web.Domain.ProductAggregate;

namespace MinimalClean.Architecture.Web.Domain.OrderAggregate;

public class Order : EntityBase<Order, OrderId>, IAggregateRoot
{
    private readonly List<OrderItem> _items = new();
    private OrderStatus _status = OrderStatus.Pending;

    public const decimal MinimumOrderAmount = 10.00m;

    public Order(OrderId id, Guid guestUserId)
    {
        Id = id;
        GuestUserId = guestUserId;
    }

    public DateTimeOffset CreatedOn { get; private set; } = DateTimeOffset.UtcNow;
    public Guid GuestUserId { get; private set; }
    public DateTimeOffset? DatePaid { get; private set; }
    public string PaymentReference { get; private set; } = string.Empty;
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status => _status;

    public decimal Total => _items.Sum(i => i.UnitPrice.Value * i.Quantity.Value);

    public void AddItem(ProductId productId, Quantity quantity, Price unitPrice)
    {
        var item = new OrderItem(Id, productId, quantity, unitPrice);

        if (Items.Any(i => i.ProductId == productId))
        {
            var existingItem = Items.First(i => i.ProductId == productId);
            existingItem.IncreaseQuantity(quantity);
            return;
        }
        _items.Add(item);
    }

    public void ConfirmPayment(DateTimeOffset datePaid, string paymentReference)
    {
        Guard.Against.NullOrEmpty(paymentReference, nameof(paymentReference));
        Guard.Against.InvalidInput(_status, nameof(Status), 
            s => s == OrderStatus.Pending, 
            "Cannot confirm payment for order that is not in Pending status");

        var oldStatus = _status;
        DatePaid = datePaid;
        PaymentReference = paymentReference;
        _status = OrderStatus.Paid;

        RegisterDomainEvent(new OrderStatusChangedEvent(this, oldStatus, _status));
    }

    public void MarkAsShipped()
    {
        Guard.Against.InvalidInput(_status, nameof(Status), 
            s => s == OrderStatus.Paid, 
            "Cannot ship order that is not in Paid status");

        var oldStatus = _status;
        _status = OrderStatus.Shipped;
        RegisterDomainEvent(new OrderStatusChangedEvent(this, oldStatus, _status));
    }

    public void MarkAsDelivered()
    {
        Guard.Against.InvalidInput(_status, nameof(Status), 
            s => s == OrderStatus.Shipped, 
            "Cannot deliver order that is not in Shipped status");

        var oldStatus = _status;
        _status = OrderStatus.Delivered;
        RegisterDomainEvent(new OrderStatusChangedEvent(this, oldStatus, _status));
    }

    public void Cancel()
    {
        Guard.Against.InvalidInput(_status, nameof(Status), 
            s => s is OrderStatus.Pending or OrderStatus.Paid, 
            "Cannot cancel order that has already been shipped or delivered");

        var oldStatus = _status;
        _status = OrderStatus.Cancelled;
        RegisterDomainEvent(new OrderStatusChangedEvent(this, oldStatus, _status));
    }

    public void ValidateMinimumOrderAmount()
    {
        if (Total < MinimumOrderAmount)
        {
            throw new InvalidOperationException($"Order total ({Total:C}) is below minimum order amount ({MinimumOrderAmount:C})");
        }
    }

    public void MarkAsCreated()
    {
        RegisterDomainEvent(new OrderCreatedEvent(this));
    }
}
