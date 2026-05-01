using MinimalClean.Architecture.Web.Infrastructure.Data;

namespace MinimalClean.Architecture.FunctionalTests;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
  protected readonly HttpClient _client;
  protected readonly CustomWebApplicationFactory<Program> _factory;

  protected TestBase(CustomWebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }

  public virtual Task InitializeAsync()
  {
    return Task.CompletedTask;
  }

  public virtual async Task DisposeAsync()
  {
    await ResetDatabaseAsync();
  }

  protected async Task ResetDatabaseAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var products = dbContext.Products.ToList();
    dbContext.Products.RemoveRange(products);

    await dbContext.SaveChangesAsync();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestBase>>();
    await SeedData.PopulateTestDataAsync(dbContext, logger);
  }
}
