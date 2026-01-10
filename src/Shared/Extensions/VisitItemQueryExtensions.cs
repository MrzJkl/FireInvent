using FireInvent.Database.Models;

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

            // VisitItem has no direct searchable text fields
            // Keeping this method for future extensibility
            return query; 
        }
    }
}
