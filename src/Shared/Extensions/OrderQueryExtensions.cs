using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class OrderQueryExtensions
    {
        public static IQueryable<Order> ApplySearch(
            this IQueryable<Order> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(o =>
                EF.Functions.ILike(o.OrderIdentifier ?? "", pattern) || EF.Functions.ILike(o.Id.ToString() ?? "", pattern));
        }
    }
}
