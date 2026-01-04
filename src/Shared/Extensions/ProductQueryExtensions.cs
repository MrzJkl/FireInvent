using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class ProductQueryExtensions
    {
        public static IQueryable<Product> ApplySearch(
            this IQueryable<Product> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(p =>
                EF.Functions.ILike(p.Name, pattern) ||
                EF.Functions.ILike(p.Description ?? "", pattern) ||
                EF.Functions.ILike(p.ExternalIdentifier ?? "", pattern)
            );
        }
    }
}
