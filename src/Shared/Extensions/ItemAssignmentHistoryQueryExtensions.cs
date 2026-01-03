using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class ItemAssignmentHistoryQueryExtensions
    {
        public static IQueryable<ItemAssignmentHistory> ApplySearch(
            this IQueryable<ItemAssignmentHistory> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            // ItemAssignmentHistory has no direct searchable text fields
            // Could search by dates if needed, but keeping it empty for now
            return query;
        }
    }
}
