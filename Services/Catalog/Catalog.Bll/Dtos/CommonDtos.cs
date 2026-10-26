namespace Catalog.Bll.Dtos;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}

public record ProductQuery(
    Guid StoreId,
    Guid? CategoryId = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    string? Search = null,
    Dictionary<string, string>? Attrs = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "price",
    string SortDir = "asc")
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}
