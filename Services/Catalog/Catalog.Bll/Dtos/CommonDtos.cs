namespace Catalog.Bll.Dtos;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}

public record ProductQuery(
    Guid StoreId,
    Guid? CategoryId = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    Dictionary<string, string>? Attrs = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "price",
    string SortDir = "asc");
