using System.Net;
using MinimalClean.Architecture.Web.ProductFeatures;

namespace MinimalClean.Architecture.FunctionalTests.Products;

[Collection("Sequential")]
public class ProductSearch : TestBase
{
  public ProductSearch(CustomWebApplicationFactory<Program> factory) : base(factory)
  {
  }

  [Fact]
  public async Task ReturnsPaginatedProducts_WhenNoFilters()
  {
    var response = await _client.GetAsync("/Products/search");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    result.TotalCount.ShouldBe(10);
    result.Items.Count.ShouldBe(10);
    result.Page.ShouldBe(1);
    result.PerPage.ShouldBe(10);
  }

  [Fact]
  public async Task ReturnsPagedResults_WhenPageSizeSpecified()
  {
    var response = await _client.GetAsync("/Products/search?page=1&per_page=3");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    result.Items.Count.ShouldBe(3);
    result.TotalCount.ShouldBe(10);
    result.TotalPages.ShouldBe(4);
  }

  [Fact]
  public async Task ReturnsCorrectPage_WhenPageNumberSpecified()
  {
    var responsePage1 = await _client.GetAsync("/Products/search?page=1&per_page=3");
    var responsePage2 = await _client.GetAsync("/Products/search?page=2&per_page=3");

    responsePage1.StatusCode.ShouldBe(HttpStatusCode.OK);
    responsePage2.StatusCode.ShouldBe(HttpStatusCode.OK);

    var resultPage1 = await responsePage1.Content.ReadFromJsonAsync<ProductSearchResponse>();
    var resultPage2 = await responsePage2.Content.ReadFromJsonAsync<ProductSearchResponse>();

    resultPage1.ShouldNotBeNull();
    resultPage2.ShouldNotBeNull();

    var page1Ids = resultPage1.Items.Select(i => i.Id).ToList();
    var page2Ids = resultPage2.Items.Select(i => i.Id).ToList();

    page1Ids.Intersect(page2Ids).ShouldBeEmpty();
  }

  [Fact]
  public async Task ReturnsBadRequest_WhenInvalidPageNumber()
  {
    var response = await _client.GetAsync("/Products/search?page=0");

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ReturnsBadRequest_WhenInvalidPageSize()
  {
    var response = await _client.GetAsync("/Products/search?per_page=0");

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ReturnsEmptyResult_WhenNoMatches()
  {
    var response = await _client.GetAsync("/Products/search?keyword=NonexistentProduct12345");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    result.TotalCount.ShouldBe(0);
    result.Items.ShouldBeEmpty();
  }

  [Fact]
  public async Task SortsByIdAscending_ByDefault()
  {
    var response = await _client.GetAsync("/Products/search?per_page=20");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    var ids = result.Items.Select(i => i.Id).ToList();
    ids.ShouldBeInOrder(SortDirection.Ascending);
  }

  [Fact]
  public async Task SortsByIdDescending_WhenSpecified()
  {
    var response = await _client.GetAsync("/Products/search?sort_by=Id&sort_order=Descending&per_page=20");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    var ids = result.Items.Select(i => i.Id).ToList();
    ids.ShouldBeInOrder(SortDirection.Descending);
  }

  [Fact]
  public async Task SortsByNameAscending_WhenSpecified()
  {
    var response = await _client.GetAsync("/Products/search?sort_by=Name&sort_order=Ascending&per_page=20");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    var names = result.Items.Select(i => i.Name).ToList();
    names.ShouldBeInOrder(SortDirection.Ascending);
  }

  [Fact]
  public async Task SortsByNameDescending_WhenSpecified()
  {
    var response = await _client.GetAsync("/Products/search?sort_by=Name&sort_order=Descending&per_page=20");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    var names = result.Items.Select(i => i.Name).ToList();
    names.ShouldBeInOrder(SortDirection.Descending);
  }

  [Fact]
  public async Task SortsByPriceAscending_WhenSpecified()
  {
    var response = await _client.GetAsync("/Products/search?sort_by=UnitPrice&sort_order=Ascending&per_page=20");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    var prices = result.Items.Select(i => i.UnitPrice).ToList();
    prices.ShouldBeInOrder(SortDirection.Ascending);
  }

  [Fact]
  public async Task SortsByPriceDescending_WhenSpecified()
  {
    var response = await _client.GetAsync("/Products/search?sort_by=UnitPrice&sort_order=Descending&per_page=20");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ProductSearchResponse>();

    result.ShouldNotBeNull();
    var prices = result.Items.Select(i => i.UnitPrice).ToList();
    prices.ShouldBeInOrder(SortDirection.Descending);
  }

  [Fact]
  public async Task ReturnsBadRequest_WhenMinPriceGreaterThanMaxPrice()
  {
    var response = await _client.GetAsync("/Products/search?min_price=100&max_price=50");

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ReturnsBadRequest_WhenNegativeMinPrice()
  {
    var response = await _client.GetAsync("/Products/search?min_price=-10");

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ReturnsLinkHeader_WhenMultiplePages()
  {
    var response = await _client.GetAsync("/Products/search?page=2&per_page=3");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    response.Headers.TryGetValues("Link", out var linkHeaders).ShouldBeTrue();

    var linkHeader = linkHeaders.ShouldNotBeNull().Single();
    linkHeader.ShouldContain("rel=\"first\"");
    linkHeader.ShouldContain("rel=\"prev\"");
    linkHeader.ShouldContain("rel=\"next\"");
    linkHeader.ShouldContain("rel=\"last\"");
  }
}

public record ProductSearchResponse
{
  public List<ProductRecord> Items { get; init; } = new();
  public int Page { get; init; }
  public int PerPage { get; init; }
  public int TotalCount { get; init; }
  public int TotalPages { get; init; }
}
