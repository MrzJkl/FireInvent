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

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(h =>
                EF.Functions.ILike(h.Id.ToString() ?? "", pattern)
            );
        }
    }
}
