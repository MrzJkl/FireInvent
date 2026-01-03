using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class MaintenanceTypeQueryExtensions
    {
        public static IQueryable<MaintenanceType> ApplySearch(
            this IQueryable<MaintenanceType> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(mt =>
                EF.Functions.ILike(mt.Name, pattern) ||
                EF.Functions.ILike(mt.Description ?? "", pattern)
            );
        }
    }
}
