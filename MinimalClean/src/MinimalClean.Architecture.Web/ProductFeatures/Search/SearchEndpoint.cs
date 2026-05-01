using FastEndpoints;
using FluentValidation;
using MinimalClean.Architecture.Web.ProductFeatures;

namespace MinimalClean.Architecture.Web.ProductFeatures.Search;

public sealed class SearchProductsRequest
{
  [BindFrom("keyword")]
  public string? Keyword { get; init; }

  [BindFrom("min_price")]
  public decimal? MinPrice { get; init; }

  [BindFrom("max_price")]
  public decimal? MaxPrice { get; init; }

  [BindFrom("page")]
  public int Page { get; init; } = 1;

  [BindFrom("per_page")]
  public int PerPage { get; init; } = Constants.DEFAULT_PAGE_SIZE;

  [BindFrom("sort_by")]
  public SortBy SortBy { get; init; } = SortBy.Id;

  [BindFrom("sort_order")]
  public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
}

public record ProductSearchResponse : PagedResult<ProductRecord>
{
  public ProductSearchResponse(IReadOnlyList<ProductRecord> Items, int Page, int PerPage, int TotalCount, int TotalPages)
    : base(Items, Page, PerPage, TotalCount, TotalPages)
  {
  }
}

public class SearchEndpoint(IMediator mediator) : Endpoint<SearchProductsRequest, ProductSearchResponse, SearchProductsMapper>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/Products/search");
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Search products with filters";
      s.Description = "Searches products with support for keyword filtering, price range, pagination, and sorting.";
      s.ExampleRequest = new SearchProductsRequest
      {
        Keyword = "Laptop",
        MinPrice = 100,
        MaxPrice = 1000,
        Page = 1,
        PerPage = 10,
        SortBy = SortBy.UnitPrice,
        SortOrder = SortOrder.Ascending
      };
      s.ResponseExamples[200] = new ProductSearchResponse(
        new List<ProductRecord>
        {
          new(1, "Gaming Laptop", 899.99m),
          new(2, "Business Laptop", 699.99m)
        },
        1, 10, 2, 1);

      s.Params["keyword"] = "Keyword to search in product names (case-insensitive)";
      s.Params["min_price"] = "Minimum price filter";
      s.Params["max_price"] = "Maximum price filter";
      s.Params["page"] = "1-based page index (default 1)";
      s.Params["per_page"] = $"Page size 1–{Constants.MAX_PAGE_SIZE} (default {Constants.DEFAULT_PAGE_SIZE})";
      s.Params["sort_by"] = "Sort field: Id, Name, or UnitPrice (default Id)";
      s.Params["sort_order"] = "Sort order: Ascending or Descending (default Ascending)";

      s.Responses[200] = "Search results returned successfully";
      s.Responses[400] = "Invalid search parameters";
    });

    Tags("Products");

    Description(builder => builder
      .Accepts<SearchProductsRequest>()
      .Produces<ProductSearchResponse>(200, "application/json")
      .ProducesProblem(400));
  }

  public override async Task HandleAsync(SearchProductsRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new SearchProductsQuery(
      Keyword: request.Keyword,
      MinPrice: request.MinPrice,
      MaxPrice: request.MaxPrice,
      Page: request.Page,
      PerPage: request.PerPage,
      SortBy: request.SortBy,
      SortOrder: request.SortOrder), cancellationToken);

    if (!result.IsSuccess)
    {
      await Send.ErrorsAsync(statusCode: 400, cancellationToken);
      return;
    }

    var pagedResult = result.Value;
    AddLinkHeader(pagedResult.Page, pagedResult.PerPage, pagedResult.TotalPages, request);

    var response = Map.FromEntity(pagedResult);
    await Send.OkAsync(response, cancellationToken);
  }

  private void AddLinkHeader(int page, int perPage, int totalPages, SearchProductsRequest request)
  {
    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
    var queryParams = BuildQueryParams(request);

    string Link(string rel, int p)
    {
      var allParams = new List<string>(queryParams) { $"page={p}", $"per_page={perPage}" };
      return $"<{baseUrl}?{string.Join("&", allParams)}>; rel=\"{rel}\"";
    }

    var parts = new List<string>();
    if (page > 1)
    {
      parts.Add(Link("first", 1));
      parts.Add(Link("prev", page - 1));
    }
    if (page < totalPages)
    {
      parts.Add(Link("next", page + 1));
      parts.Add(Link("last", totalPages));
    }

    if (parts.Count > 0)
      HttpContext.Response.Headers["Link"] = string.Join(", ", parts);
  }

  private List<string> BuildQueryParams(SearchProductsRequest request)
  {
    var paramsList = new List<string>();

    if (!string.IsNullOrWhiteSpace(request.Keyword))
      paramsList.Add($"keyword={Uri.EscapeDataString(request.Keyword)}");

    if (request.MinPrice.HasValue)
      paramsList.Add($"min_price={request.MinPrice.Value}");

    if (request.MaxPrice.HasValue)
      paramsList.Add($"max_price={request.MaxPrice.Value}");

    if (request.SortBy != SortBy.Id)
      paramsList.Add($"sort_by={request.SortBy}");

    if (request.SortOrder != SortOrder.Ascending)
      paramsList.Add($"sort_order={request.SortOrder}");

    return paramsList;
  }
}

public sealed class SearchProductsValidator : Validator<SearchProductsRequest>
{
  public SearchProductsValidator()
  {
    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1)
      .WithMessage("page must be >= 1");

    RuleFor(x => x.PerPage)
      .InclusiveBetween(1, Constants.MAX_PAGE_SIZE)
      .WithMessage($"per_page must be between 1 and {Constants.MAX_PAGE_SIZE}");

    RuleFor(x => x.MinPrice)
      .GreaterThanOrEqualTo(0)
      .When(x => x.MinPrice.HasValue)
      .WithMessage("min_price must be >= 0");

    RuleFor(x => x.MaxPrice)
      .GreaterThanOrEqualTo(0)
      .When(x => x.MaxPrice.HasValue)
      .WithMessage("max_price must be >= 0");

    RuleFor(x => x)
      .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
      .WithMessage("min_price must be <= max_price");

    RuleFor(x => x.SortBy)
      .IsInEnum()
      .WithMessage("sort_by must be one of: Id, Name, UnitPrice");

    RuleFor(x => x.SortOrder)
      .IsInEnum()
      .WithMessage("sort_order must be one of: Ascending, Descending");
  }
}

public sealed class SearchProductsMapper
  : Mapper<SearchProductsRequest, ProductSearchResponse, PagedResult<ProductDto>>
{
  public override ProductSearchResponse FromEntity(PagedResult<ProductDto> e)
  {
    var items = e.Items
      .Select(p => new ProductRecord(p.Id.Value, p.Name, p.UnitPrice))
      .ToList();

    return new ProductSearchResponse(items, e.Page, e.PerPage, e.TotalCount, e.TotalPages);
  }
}
