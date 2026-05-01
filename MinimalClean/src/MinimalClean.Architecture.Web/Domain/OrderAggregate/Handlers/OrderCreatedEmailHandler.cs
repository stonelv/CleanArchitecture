using Ardalis.GuardClauses;
using MinimalClean.Architecture.Web.Domain.GuestUserAggregate;
using MinimalClean.Architecture.Web.Domain.GuestUserAggregate.Specifications;
using MinimalClean.Architecture.Web.Domain.Interfaces;
using MinimalClean.Architecture.Web.Domain.OrderAggregate.Events;

namespace MinimalClean.Architecture.Web.Domain.OrderAggregate.Handlers;

public class OrderCreatedEmailHandler(
    IEmailSender emailSender,
    IRepository<GuestUser> guestUserRepository)
    : INotificationHandler<OrderCreatedEvent>
{
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IRepository<GuestUser> _guestUserRepository = guestUserRepository;

    public async ValueTask Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var order = notification.Order;
        
        var guestUserSpec = new GuestUserByIdSpec(GuestUserId.From(order.GuestUserId));
        var guestUser = await _guestUserRepository.FirstOrDefaultAsync(guestUserSpec, cancellationToken);

        if (guestUser == null)
        {
            return;
        }

        var subject = $"Your Order Confirmation - Order #{order.Id.Value}";
        var body = $@"
Thank you for your order!

Order Details:
- Order ID: {order.Id.Value}
- Order Date: {order.CreatedOn:yyyy-MM-dd HH:mm:ss}
- Status: {order.Status}
- Total Amount: {order.Total:C}

Order Items:
{string.Join(Environment.NewLine, order.Items.Select(item => 
    $"- Product: {item.ProductId.Value}, Quantity: {item.Quantity.Value}, Price: {item.UnitPrice.Value:C}"))}

Thank you for your purchase!
";

        await _emailSender.SendEmailAsync(
            to: guestUser.Email,
            from: "orders@example.com",
            subject: subject,
            body: body);
    }
}
