namespace MinimalClean.Architecture.Web.ProductFeatures.Search;

public record SearchProductsQuery(
  string? Keyword = null,
  decimal? MinPrice = null,
  decimal? MaxPrice = null,
  int? Page = 1,
  int? PerPage = Constants.DEFAULT_PAGE_SIZE,
  SortBy? SortBy = SortBy.Id,
  SortOrder? SortOrder = SortOrder.Ascending)
  : IQuery<Result<PagedResult<ProductDto>>>;

public class SearchProductsHandler(ISearchProductsQueryService queryService)
  : IQueryHandler<SearchProductsQuery, Result<PagedResult<ProductDto>>>
{
  private readonly ISearchProductsQueryService _queryService = queryService;

  public async ValueTask<Result<PagedResult<ProductDto>>> Handle(SearchProductsQuery request,
                                                                 CancellationToken cancellationToken)
  {
    var criteria = new SearchProductsCriteria(
      Keyword: request.Keyword,
      MinPrice: request.MinPrice,
      MaxPrice: request.MaxPrice,
      Page: request.Page ?? 1,
      PerPage: request.PerPage ?? Constants.DEFAULT_PAGE_SIZE,
      SortBy: request.SortBy ?? SortBy.Id,
      SortOrder: request.SortOrder ?? SortOrder.Ascending);

    var result = await _queryService.SearchAsync(criteria, cancellationToken);

    return Result.Success(result);
  }
}
