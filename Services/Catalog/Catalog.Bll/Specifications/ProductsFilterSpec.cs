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

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = $"%{q.Search.Trim()}%";
            Query.Where(p => EF.Functions.ILike(p.Name, term) || EF.Functions.ILike(p.Sku, term));
        }

        if (q.Attrs != null && q.Attrs.Count > 0)
        {
            foreach (var (key, rawValues) in q.Attrs)
            {
                if (string.IsNullOrWhiteSpace(rawValues)) continue;

                var normalizedKey = key.Trim().ToLowerInvariant();
                var values = rawValues.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                var snippets = values.Take(5).Select(v => $"{{\"{normalizedKey}\": \"{v}\"}}").ToArray();
                if (snippets.Length == 1)
                {
                    var s0 = snippets[0];
                    Query.Where(p => p.Detail != null && EF.Functions.JsonContains(p.Detail.Characteristics, s0));
                }
                else if (snippets.Length == 2)
                {
                    var s0 = snippets[0]; var s1 = snippets[1];
                    Query.Where(p => p.Detail != null && (EF.Functions.JsonContains(p.Detail.Characteristics, s0) || EF.Functions.JsonContains(p.Detail.Characteristics, s1)));
                }
                else if (snippets.Length == 3)
                {
                    var s0 = snippets[0]; var s1 = snippets[1]; var s2 = snippets[2];
                    Query.Where(p => p.Detail != null && (EF.Functions.JsonContains(p.Detail.Characteristics, s0) || EF.Functions.JsonContains(p.Detail.Characteristics, s1) || EF.Functions.JsonContains(p.Detail.Characteristics, s2)));
                }
                else if (snippets.Length >= 4)
                {
                    var s0 = snippets[0]; var s1 = snippets[1]; var s2 = snippets[2]; var s3 = snippets[3];
                    Query.Where(p => p.Detail != null && (EF.Functions.JsonContains(p.Detail.Characteristics, s0) || EF.Functions.JsonContains(p.Detail.Characteristics, s1) || EF.Functions.JsonContains(p.Detail.Characteristics, s2) || EF.Functions.JsonContains(p.Detail.Characteristics, s3)));
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
                    Query.OrderByDescending(p => p.Price).ThenBy(p => p.Id);
                else
                    Query.OrderBy(p => p.Price).ThenBy(p => p.Id);
            }
            else
            {
                if (isDesc)
                    Query.OrderByDescending(p => p.Name).ThenBy(p => p.Id);
                else
                    Query.OrderBy(p => p.Name).ThenBy(p => p.Id);
            }

            Query.Skip((q.Page - 1) * q.PageSize)
                 .Take(q.PageSize);

            Query.Include(p => p.Detail)
                 .Include(p => p.Images.OrderBy(i => i.DisplayOrder));
        }
    }
}
