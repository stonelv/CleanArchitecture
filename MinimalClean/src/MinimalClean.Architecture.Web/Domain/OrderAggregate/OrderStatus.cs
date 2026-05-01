namespace MinimalClean.Architecture.Web.Domain.OrderAggregate;

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}
