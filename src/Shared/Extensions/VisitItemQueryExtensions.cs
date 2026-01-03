using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class VisitItemQueryExtensions
    {
        public static IQueryable<VisitItem> ApplySearch(
            this IQueryable<VisitItem> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            // VisitItem has no direct searchable text fields
            // Keeping this method for future extensibility
            return query; 
        }
    }
}
