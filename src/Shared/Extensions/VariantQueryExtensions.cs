using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class VariantQueryExtensions
    {
        public static IQueryable<Variant> ApplySearch(
            this IQueryable<Variant> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(v =>
                EF.Functions.ILike(v.Name, pattern) ||
                EF.Functions.ILike(v.AdditionalSpecs ?? "", pattern) ||
                EF.Functions.ILike(v.ExternalIdentifier ?? "", pattern) ||
                EF.Functions.ILike(v.Id.ToString() ?? "", pattern)
            );
        }
    }
}
