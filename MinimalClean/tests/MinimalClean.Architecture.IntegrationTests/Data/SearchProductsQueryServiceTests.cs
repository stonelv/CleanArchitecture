using MinimalClean.Architecture.Web.Domain.ProductAggregate;
using MinimalClean.Architecture.Web.Infrastructure.Data.Queries;
using MinimalClean.Architecture.Web.ProductFeatures.Search;

namespace MinimalClean.Architecture.IntegrationTests.Data;

public class SearchProductsQueryServiceTests : BaseEfTestFixture
{
  private readonly SearchProductsQueryService _queryService;

  public SearchProductsQueryServiceTests()
  {
    _queryService = new SearchProductsQueryService(_dbContext);
    SeedTestData();
  }

  private void SeedTestData()
  {
    var products = new List<Product>
    {
      Product.Create("Apple iPhone", 999.99m),
      Product.Create("Apple MacBook", 1999.99m),
      Product.Create("Samsung Galaxy", 799.99m),
      Product.Create("Google Pixel", 699.99m),
      Product.Create("Apple Watch", 399.99m),
      Product.Create("Sony Headphones", 299.99m),
      Product.Create("Apple AirPods", 199.99m),
      Product.Create("Samsung Tablet", 499.99m)
    };

    _dbContext.Products.AddRange(products);
    _dbContext.SaveChanges();
  }

  [Fact]
  public async Task SearchAsync_ReturnsAllProducts_WhenNoFilters()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.ShouldNotBeNull();
    result.TotalCount.ShouldBe(8);
    result.Items.Count.ShouldBe(8);
  }

  [Fact]
  public async Task SearchAsync_FiltersByKeyword()
  {
    var criteria = new SearchProductsCriteria(Keyword: "Apple", Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.TotalCount.ShouldBe(4);
    result.Items.ShouldAllBe(p => p.Name.Contains("Apple"));
  }

  [Fact]
  public async Task SearchAsync_FiltersByMinPrice()
  {
    var criteria = new SearchProductsCriteria(MinPrice: 500, Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.TotalCount.ShouldBe(4);
    result.Items.ShouldAllBe(p => p.UnitPrice >= 500);
  }

  [Fact]
  public async Task SearchAsync_FiltersByMaxPrice()
  {
    var criteria = new SearchProductsCriteria(MaxPrice: 500, Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.TotalCount.ShouldBe(4);
    result.Items.ShouldAllBe(p => p.UnitPrice <= 500);
  }

  [Fact]
  public async Task SearchAsync_FiltersByPriceRange()
  {
    var criteria = new SearchProductsCriteria(MinPrice: 300, MaxPrice: 1000, Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.TotalCount.ShouldBe(5);
    result.Items.ShouldAllBe(p => p.UnitPrice >= 300 && p.UnitPrice <= 1000);
  }

  [Fact]
  public async Task SearchAsync_CombinesKeywordAndPriceFilter()
  {
    var criteria = new SearchProductsCriteria(Keyword: "Apple", MinPrice: 500, Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.TotalCount.ShouldBe(2);
    result.Items.ShouldAllBe(p => p.Name.Contains("Apple") && p.UnitPrice >= 500);
  }

  [Fact]
  public async Task SearchAsync_ReturnsPagedResults()
  {
    var criteriaPage1 = new SearchProductsCriteria(Page: 1, PerPage: 3);
    var criteriaPage2 = new SearchProductsCriteria(Page: 2, PerPage: 3);

    var resultPage1 = await _queryService.SearchAsync(criteriaPage1);
    var resultPage2 = await _queryService.SearchAsync(criteriaPage2);

    resultPage1.Items.Count.ShouldBe(3);
    resultPage1.TotalCount.ShouldBe(8);
    resultPage1.TotalPages.ShouldBe(3);

    resultPage2.Items.Count.ShouldBe(3);
  }

  [Fact]
  public async Task SearchAsync_SortsByIdAscending()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20, SortBy: SortBy.Id, SortOrder: SortOrder.Ascending);

    var result = await _queryService.SearchAsync(criteria);

    var ids = result.Items.Select(p => p.Id.Value).ToList();
    ids.ShouldBeInOrder(SortDirection.Ascending);
  }

  [Fact]
  public async Task SearchAsync_SortsByIdDescending()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20, SortBy: SortBy.Id, SortOrder: SortOrder.Descending);

    var result = await _queryService.SearchAsync(criteria);

    var ids = result.Items.Select(p => p.Id.Value).ToList();
    ids.ShouldBeInOrder(SortDirection.Descending);
  }

  [Fact]
  public async Task SearchAsync_SortsByNameAscending()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20, SortBy: SortBy.Name, SortOrder: SortOrder.Ascending);

    var result = await _queryService.SearchAsync(criteria);

    var names = result.Items.Select(p => p.Name).ToList();
    names.ShouldBeInOrder(SortDirection.Ascending);
  }

  [Fact]
  public async Task SearchAsync_SortsByNameDescending()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20, SortBy: SortBy.Name, SortOrder: SortOrder.Descending);

    var result = await _queryService.SearchAsync(criteria);

    var names = result.Items.Select(p => p.Name).ToList();
    names.ShouldBeInOrder(SortDirection.Descending);
  }

  [Fact]
  public async Task SearchAsync_SortsByPriceAscending()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20, SortBy: SortBy.UnitPrice, SortOrder: SortOrder.Ascending);

    var result = await _queryService.SearchAsync(criteria);

    var prices = result.Items.Select(p => p.UnitPrice).ToList();
    prices.ShouldBeInOrder(SortDirection.Ascending);
  }

  [Fact]
  public async Task SearchAsync_SortsByPriceDescending()
  {
    var criteria = new SearchProductsCriteria(Page: 1, PerPage: 20, SortBy: SortBy.UnitPrice, SortOrder: SortOrder.Descending);

    var result = await _queryService.SearchAsync(criteria);

    var prices = result.Items.Select(p => p.UnitPrice).ToList();
    prices.ShouldBeInOrder(SortDirection.Descending);
  }

  [Fact]
  public async Task SearchAsync_ReturnsEmpty_WhenNoMatches()
  {
    var criteria = new SearchProductsCriteria(Keyword: "NonexistentProduct", Page: 1, PerPage: 20);

    var result = await _queryService.SearchAsync(criteria);

    result.TotalCount.ShouldBe(0);
    result.Items.ShouldBeEmpty();
  }
}
