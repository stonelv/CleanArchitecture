using Ardalis.GuardClauses;

namespace MinimalClean.Architecture.Web.Domain.ProductAggregate;

public class Product : EntityBase<Product, ProductId>, IAggregateRoot
{
    private int _stockQuantity;

    private Product() { }

    private Product(string name, decimal unitPrice, int stockQuantity = 0)
    {
        Name = name;
        UnitPrice = unitPrice;
        _stockQuantity = stockQuantity;
    }

    public Product(ProductId id, string name, decimal unitPrice, int stockQuantity = 0)
    {
        Guard.Against.InvalidInput(id, nameof(id), (id) => id != ProductId.New,
            "Use Product.Create() to create new products instead of passing ProductId.New to the constructor.");
        Id = id;
        Name = name;
        UnitPrice = unitPrice;
        _stockQuantity = stockQuantity;
    }

    public static Product Create(string name, decimal unitPrice, int stockQuantity = 0) => new Product(name, unitPrice, stockQuantity);

    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int StockQuantity => _stockQuantity;

    public Product UpdateName(string newName)
    {
        Name = newName;
        return this;
    }

    public Product UpdatePrice(decimal newPrice)
    {
        UnitPrice = newPrice;
        return this;
    }

    public void AddStock(int quantity)
    {
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));
        _stockQuantity += quantity;
    }

    public void RemoveStock(int quantity)
    {
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));
        if (_stockQuantity < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {_stockQuantity}, Requested: {quantity}");
        }
        _stockQuantity -= quantity;
    }

    public bool HasSufficientStock(int quantity)
    {
        return _stockQuantity >= quantity;
    }
}
