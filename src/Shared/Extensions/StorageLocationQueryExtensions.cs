using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class StorageLocationQueryExtensions
    {
        public static IQueryable<StorageLocation> ApplySearch(
            this IQueryable<StorageLocation> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(sl =>
                EF.Functions.ILike(sl.Name, pattern) ||
                EF.Functions.ILike(sl.Remarks ?? "", pattern) ||
                EF.Functions.ILike(sl.Id.ToString() ?? "", pattern)
            );
        }
    }
}
