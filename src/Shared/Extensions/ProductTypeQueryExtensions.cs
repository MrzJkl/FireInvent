using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class ProductTypeQueryExtensions
    {
        public static IQueryable<ProductType> ApplySearch(
            this IQueryable<ProductType> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(pt =>
                EF.Functions.ILike(pt.Name, pattern) ||
                EF.Functions.ILike(pt.Description ?? "", pattern) ||
                EF.Functions.ILike(pt.Id.ToString() ?? "", pattern)
            );
        }
    }
}
