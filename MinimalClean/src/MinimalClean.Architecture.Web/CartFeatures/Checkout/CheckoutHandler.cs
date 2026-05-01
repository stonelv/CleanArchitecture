using MinimalClean.Architecture.Web.Domain.CartAggregate;
using MinimalClean.Architecture.Web.Domain.CartAggregate.Specifications;
using MinimalClean.Architecture.Web.Domain.GuestUserAggregate;
using MinimalClean.Architecture.Web.Domain.GuestUserAggregate.Specifications;
using MinimalClean.Architecture.Web.Domain.OrderAggregate;
using MinimalClean.Architecture.Web.Domain.ProductAggregate;
using MinimalClean.Architecture.Web.Domain.ProductAggregate.Specifications;

namespace MinimalClean.Architecture.Web.CartFeatures.Checkout;

public record CheckoutCommand(CartId CartId, string Email) : ICommand<Result<CheckoutResult>>;

public record CheckoutResult(OrderId OrderId);

public class CheckoutHandler(
    IRepository<Domain.CartAggregate.Cart> cartRepository,
    IRepository<GuestUser> guestUserRepository,
    IRepository<Order> orderRepository,
    IRepository<Product> productRepository)
    : ICommandHandler<CheckoutCommand, Result<CheckoutResult>>
{
    public async ValueTask<Result<CheckoutResult>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cartSpec = new CartByIdSpec(request.CartId);
        var cart = await cartRepository.FirstOrDefaultAsync(cartSpec, cancellationToken);

        if (cart == null) return Result.NotFound("Cart not found");
        if (!cart.Items.Any()) return Result.Invalid(new ValidationError("Cart is empty"));

        var guestUserSpec = new GuestUserByEmailSpec(request.Email);
        var guestUser = await guestUserRepository.FirstOrDefaultAsync(guestUserSpec, cancellationToken);

        if (guestUser == null)
        {
            var guestUserId = GuestUserId.From(Guid.NewGuid());
            guestUser = new GuestUser(guestUserId, request.Email);
            guestUser = await guestUserRepository.AddAsync(guestUser, cancellationToken);
        }

        var stockValidationErrors = new List<ValidationError>();
        foreach (var cartItem in cart.Items)
        {
            var productSpec = new ProductByIdSpec(ProductId.From(cartItem.ProductId));
            var product = await productRepository.FirstOrDefaultAsync(productSpec, cancellationToken);

            if (product == null)
            {
                stockValidationErrors.Add(new ValidationError($"Product with ID {cartItem.ProductId} not found"));
                continue;
            }

            if (!product.HasSufficientStock(cartItem.Quantity))
            {
                stockValidationErrors.Add(new ValidationError(
                    $"Insufficient stock for product '{product.Name}'. " +
                    $"Available: {product.StockQuantity}, Requested: {cartItem.Quantity}"));
            }
        }

        if (stockValidationErrors.Any())
        {
            return Result.Invalid(stockValidationErrors);
        }

        var orderId = OrderId.From(Guid.NewGuid());
        var order = new Order(orderId, guestUser.Id.Value);

        foreach (var cartItem in cart.Items)
        {
            order.AddItem(
                ProductId.From(cartItem.ProductId),
                Quantity.From(cartItem.Quantity),
                Price.From(cartItem.UnitPrice));
        }

        try
        {
            order.ValidateMinimumOrderAmount();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Invalid(new ValidationError(ex.Message));
        }

        foreach (var cartItem in cart.Items)
        {
            var productSpec = new ProductByIdSpec(ProductId.From(cartItem.ProductId));
            var product = await productRepository.FirstOrDefaultAsync(productSpec, cancellationToken);
            
            if (product != null)
            {
                product.RemoveStock(cartItem.Quantity);
                await productRepository.UpdateAsync(product, cancellationToken);
            }
        }

        order.MarkAsCreated();

        await orderRepository.AddAsync(order, cancellationToken);

        cart.MarkAsDeleted();
        await cartRepository.UpdateAsync(cart, cancellationToken);

        return new CheckoutResult(order.Id);
    }
}
