using System.Net;
using System.Net.Http.Json;
using Ardalis.HttpClientTestExtensions;
using Microsoft.Extensions.DependencyInjection;
using MinimalClean.Architecture.Web.CartFeatures;
using MinimalClean.Architecture.Web.CartFeatures.AddToCart;
using MinimalClean.Architecture.Web.CartFeatures.Checkout;
using MinimalClean.Architecture.Web.Domain.ProductAggregate;
using MinimalClean.Architecture.Web.Infrastructure.Data;
using Shouldly;
using Xunit;

namespace MinimalClean.Architecture.FunctionalTests;

[Collection("Sequential")]
public class CheckoutEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CheckoutEndpointTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Checkout_WithValidCartAndSufficientStock_ReturnsSuccess()
    {
        var productId = 101;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Products.Add(new Product(ProductId.From(productId), "Test Product", 15.99m, 10));
            await db.SaveChangesAsync();
        }

        var addToCartRequest = new AddToCartRequest
        {
            CartId = null,
            ProductId = productId,
            Quantity = 2
        };

        var addResponse = await _client.PostAsJsonAsync("/cart", addToCartRequest);
        addResponse.EnsureSuccessStatusCode();
        var cartResponse = await addResponse.Content.ReadFromJsonAsync<CartResponse>();
        cartResponse.ShouldNotBeNull();

        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartResponse.CartId,
            Email = "test@example.com"
        };

        var checkoutResponse = await _client.PostAsJsonAsync($"/cart/{cartResponse.CartId}/checkout", checkoutRequest);

        checkoutResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var checkoutResult = await checkoutResponse.Content.ReadFromJsonAsync<CheckoutResponse>();
        checkoutResult.ShouldNotBeNull();
        checkoutResult.OrderId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Checkout_WithInsufficientStock_ReturnsValidationError()
    {
        var productId = 201;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Products.Add(new Product(ProductId.From(productId), "Limited Stock Product", 25.99m, 1));
            await db.SaveChangesAsync();
        }

        var addToCartRequest = new AddToCartRequest
        {
            CartId = null,
            ProductId = productId,
            Quantity = 5
        };

        var addResponse = await _client.PostAsJsonAsync("/cart", addToCartRequest);
        addResponse.EnsureSuccessStatusCode();
        var cartResponse = await addResponse.Content.ReadFromJsonAsync<CartResponse>();
        cartResponse.ShouldNotBeNull();

        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartResponse.CartId,
            Email = "test@example.com"
        };

        var checkoutResponse = await _client.PostAsJsonAsync($"/cart/{cartResponse.CartId}/checkout", checkoutRequest);

        checkoutResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await checkoutResponse.Content.ReadAsStringAsync();
        content.ShouldContain("Insufficient stock");
    }

    [Fact]
    public async Task Checkout_WithOrderBelowMinimumAmount_ReturnsValidationError()
    {
        var productId = 301;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Products.Add(new Product(ProductId.From(productId), "Cheap Product", 5.00m, 100));
            await db.SaveChangesAsync();
        }

        var addToCartRequest = new AddToCartRequest
        {
            CartId = null,
            ProductId = productId,
            Quantity = 1
        };

        var addResponse = await _client.PostAsJsonAsync("/cart", addToCartRequest);
        addResponse.EnsureSuccessStatusCode();
        var cartResponse = await addResponse.Content.ReadFromJsonAsync<CartResponse>();
        cartResponse.ShouldNotBeNull();

        var checkoutRequest = new CheckoutRequest
        {
            CartId = cartResponse.CartId,
            Email = "test@example.com"
        };

        var checkoutResponse = await _client.PostAsJsonAsync($"/cart/{cartResponse.CartId}/checkout", checkoutRequest);

        checkoutResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await checkoutResponse.Content.ReadAsStringAsync();
        content.ShouldContain("minimum order amount");
    }

    [Fact]
    public async Task Checkout_WithNonExistentCart_ReturnsNotFound()
    {
        var emptyCartId = Guid.NewGuid();
        var checkoutRequest = new CheckoutRequest
        {
            CartId = emptyCartId,
            Email = "test@example.com"
        };

        var checkoutResponse = await _client.PostAsJsonAsync($"/cart/{emptyCartId}/checkout", checkoutRequest);

        checkoutResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
