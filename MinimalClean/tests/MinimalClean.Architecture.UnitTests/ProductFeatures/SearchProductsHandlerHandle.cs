using MinimalClean.Architecture.Web;
using MinimalClean.Architecture.Web.Domain.ProductAggregate;
using MinimalClean.Architecture.Web.ProductFeatures;
using MinimalClean.Architecture.Web.ProductFeatures.Search;
using WebPagedResult = MinimalClean.Architecture.Web.PagedResult<MinimalClean.Architecture.Web.ProductFeatures.ProductDto>;

namespace MinimalClean.Architecture.UnitTests.ProductFeatures;

public class SearchProductsHandlerHandle
{
  private readonly ISearchProductsQueryService _queryService = Substitute.For<ISearchProductsQueryService>();
  private readonly SearchProductsHandler _handler;

  public SearchProductsHandlerHandle()
  {
    _handler = new SearchProductsHandler(_queryService);
  }

  [Fact]
  public async Task ReturnsSuccessResultWithPagedProducts()
  {
    var expectedProducts = new List<ProductDto>
    {
      new(ProductId.From(1), "Product A", 10.99m),
      new(ProductId.From(2), "Product B", 20.99m)
    };
    var expectedPagedResult = new WebPagedResult(expectedProducts, 1, 10, 2, 1);

    _queryService.SearchAsync(Arg.Any<SearchProductsCriteria>(), Arg.Any<CancellationToken>())
      .Returns(expectedPagedResult);

    var result = await _handler.Handle(new SearchProductsQuery(), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldNotBeNull();
    result.Value.Items.Count.ShouldBe(2);
    result.Value.TotalCount.ShouldBe(2);
    result.Value.TotalPages.ShouldBe(1);
  }

  [Fact]
  public async Task PassesCorrectCriteriaToQueryService()
  {
    SearchProductsCriteria? capturedCriteria = null;
    _queryService.SearchAsync(Arg.Do<SearchProductsCriteria>(c => capturedCriteria = c), Arg.Any<CancellationToken>())
      .Returns(new WebPagedResult(new List<ProductDto>(), 1, 10, 0, 0));

    var query = new SearchProductsQuery(
      Keyword: "test",
      MinPrice: 10,
      MaxPrice: 100,
      Page: 2,
      PerPage: 20,
      SortBy: SortBy.Name,
      SortOrder: SortOrder.Descending);

    await _handler.Handle(query, CancellationToken.None);

    capturedCriteria.ShouldNotBeNull();
    capturedCriteria.Keyword.ShouldBe("test");
    capturedCriteria.MinPrice.ShouldBe(10);
    capturedCriteria.MaxPrice.ShouldBe(100);
    capturedCriteria.Page.ShouldBe(2);
    capturedCriteria.PerPage.ShouldBe(20);
    capturedCriteria.SortBy.ShouldBe(SortBy.Name);
    capturedCriteria.SortOrder.ShouldBe(SortOrder.Descending);
  }

  [Fact]
  public async Task UsesDefaultValuesWhenNotProvided()
  {
    SearchProductsCriteria? capturedCriteria = null;
    _queryService.SearchAsync(Arg.Do<SearchProductsCriteria>(c => capturedCriteria = c), Arg.Any<CancellationToken>())
      .Returns(new WebPagedResult(new List<ProductDto>(), 1, 10, 0, 0));

    var query = new SearchProductsQuery();

    await _handler.Handle(query, CancellationToken.None);

    capturedCriteria.ShouldNotBeNull();
    capturedCriteria.Keyword.ShouldBeNull();
    capturedCriteria.MinPrice.ShouldBeNull();
    capturedCriteria.MaxPrice.ShouldBeNull();
    capturedCriteria.Page.ShouldBe(1);
    capturedCriteria.PerPage.ShouldBe(Constants.DEFAULT_PAGE_SIZE);
    capturedCriteria.SortBy.ShouldBe(SortBy.Id);
    capturedCriteria.SortOrder.ShouldBe(SortOrder.Ascending);
  }

  [Fact]
  public async Task ReturnsEmptyResultWhenNoProductsMatch()
  {
    var emptyPagedResult = new WebPagedResult(new List<ProductDto>(), 1, 10, 0, 0);

    _queryService.SearchAsync(Arg.Any<SearchProductsCriteria>(), Arg.Any<CancellationToken>())
      .Returns(emptyPagedResult);

    var result = await _handler.Handle(new SearchProductsQuery(Keyword: "nonexistent"), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.Items.ShouldBeEmpty();
    result.Value.TotalCount.ShouldBe(0);
  }
}
