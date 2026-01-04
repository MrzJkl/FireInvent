using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class VisitQueryExtensions
    {
        public static IQueryable<Visit> ApplySearch(
            this IQueryable<Visit> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            // VisitItem has no direct searchable text fields
            // Keeping this method for future extensibility
            return query;
        }
    }
}
