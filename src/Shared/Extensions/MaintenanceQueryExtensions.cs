using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class MaintenanceQueryExtensions
    {
        public static IQueryable<Maintenance> ApplySearch(
            this IQueryable<Maintenance> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(m =>
                EF.Functions.ILike(m.Remarks ?? "", pattern)
            );
        }
    }
}
