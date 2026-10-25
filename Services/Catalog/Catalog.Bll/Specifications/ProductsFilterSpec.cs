using Ardalis.Specification;
using Catalog.Bll.Dtos;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Bll.Specifications;

public class ProductsFilterSpec : Specification<Product>
{
    public ProductsFilterSpec(ProductQuery q, bool applyPagingAndSorting = true)
    {
        Query.Where(p => p.IsActive && !p.IsDeleted);

        if (q.CategoryId.HasValue)
        {
            Query.Where(p => p.CategoryId == q.CategoryId.Value);
        }

        if (q.PriceMin.HasValue)
        {
            Query.Where(p => p.Price >= q.PriceMin.Value);
        }

        if (q.PriceMax.HasValue)
        {
            Query.Where(p => p.Price <= q.PriceMax.Value);
        }

        if (q.Attrs != null && q.Attrs.Count > 0)
        {
            foreach (var (key, rawValues) in q.Attrs)
            {
                if (string.IsNullOrWhiteSpace(rawValues)) continue;

                var normalizedKey = key.Trim().ToLowerInvariant();
                var values = rawValues.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (values.Length == 1)
                {
                    var snippet = $"{{\"{normalizedKey}\": \"{values[0]}\"}}";
                    Query.Where(p => p.Detail != null && EF.Functions.JsonContains(p.Detail.Characteristics, snippet));
                }
                else if (values.Length > 1)
                {
                    var snippets = values.Select(v => $"{{\"{normalizedKey}\": \"{v}\"}}").ToList();
                    Query.Where(p => p.Detail != null && snippets.Any(s => EF.Functions.JsonContains(p.Detail.Characteristics, s)));
                }
            }
        }

        if (applyPagingAndSorting)
        {
            var sortBy = q.SortBy.Trim().ToLowerInvariant();
            var isDesc = q.SortDir.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);

            if (sortBy == "price")
            {
                if (isDesc)
                    Query.OrderByDescending(p => p.Price);
                else
                    Query.OrderBy(p => p.Price);
            }
            else
            {
                if (isDesc)
                    Query.OrderByDescending(p => p.Name);
                else
                    Query.OrderBy(p => p.Name);
            }

            var page = q.Page < 1 ? 1 : q.Page;
            var pageSize = q.PageSize is < 1 or > 100 ? 20 : q.PageSize;

            Query.Skip((page - 1) * pageSize)
                 .Take(pageSize);

            Query.Include(p => p.Detail)
                 .Include(p => p.Images.OrderBy(i => i.DisplayOrder));
        }
    }
}
