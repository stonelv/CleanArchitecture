using Microsoft.EntityFrameworkCore;
using MinimalClean.Architecture.Web.ProductFeatures;
using MinimalClean.Architecture.Web.ProductFeatures.Search;

namespace MinimalClean.Architecture.Web.Infrastructure.Data.Queries;

public class SearchProductsQueryService(AppDbContext db) : ISearchProductsQueryService
{
  private readonly AppDbContext _db = db;

  public async Task<PagedResult<ProductDto>> SearchAsync(SearchProductsCriteria criteria, CancellationToken cancellationToken = default)
  {
    var query = _db.Products.AsQueryable();

    if (!string.IsNullOrWhiteSpace(criteria.Keyword))
    {
      query = query.Where(p => p.Name.Contains(criteria.Keyword));
    }

    if (criteria.MinPrice.HasValue)
    {
      query = query.Where(p => p.UnitPrice >= criteria.MinPrice.Value);
    }

    if (criteria.MaxPrice.HasValue)
    {
      query = query.Where(p => p.UnitPrice <= criteria.MaxPrice.Value);
    }

    query = criteria.SortBy switch
    {
      SortBy.Name => criteria.SortOrder == SortOrder.Ascending
        ? query.OrderBy(p => p.Name)
        : query.OrderByDescending(p => p.Name),
      SortBy.UnitPrice => criteria.SortOrder == SortOrder.Ascending
        ? query.OrderBy(p => p.UnitPrice)
        : query.OrderByDescending(p => p.UnitPrice),
      _ => criteria.SortOrder == SortOrder.Ascending
        ? query.OrderBy(p => p.Id)
        : query.OrderByDescending(p => p.Id)
    };

    int totalCount = await query.CountAsync(cancellationToken);

    var items = await query
      .Skip((criteria.Page - 1) * criteria.PerPage)
      .Take(criteria.PerPage)
      .Select(p => new ProductDto(p.Id, p.Name, p.UnitPrice))
      .AsNoTracking()
      .ToListAsync(cancellationToken);

    int totalPages = (int)Math.Ceiling(totalCount / (double)criteria.PerPage);
    var result = new PagedResult<ProductDto>(items, criteria.Page, criteria.PerPage, totalCount, totalPages);

    return result;
  }
}
