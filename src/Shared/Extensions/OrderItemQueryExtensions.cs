using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class OrderItemQueryExtensions
    {
        public static IQueryable<OrderItem> ApplySearch(
            this IQueryable<OrderItem> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            // OrderItem has no direct searchable text fields
            // Keeping this method for future extensibility
            return query;
        }
    }
}
