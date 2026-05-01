namespace MinimalClean.Architecture.Web.ProductFeatures.Search;

public enum SortBy
{
  Id,
  Name,
  UnitPrice
}

public enum SortOrder
{
  Ascending,
  Descending
}

public record SearchProductsCriteria(
  string? Keyword = null,
  decimal? MinPrice = null,
  decimal? MaxPrice = null,
  int Page = 1,
  int PerPage = Constants.DEFAULT_PAGE_SIZE,
  SortBy SortBy = SortBy.Id,
  SortOrder SortOrder = SortOrder.Ascending);

/// <summary>
/// Represents a service that will actually fetch the necessary data
/// Typically implemented in Infrastructure
/// </summary>
public interface ISearchProductsQueryService
{
  Task<PagedResult<ProductDto>> SearchAsync(SearchProductsCriteria criteria, CancellationToken cancellationToken = default);
}
