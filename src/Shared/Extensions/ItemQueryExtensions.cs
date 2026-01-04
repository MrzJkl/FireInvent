using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class ItemQueryExtensions
    {
        public static IQueryable<Item> ApplySearch(
            this IQueryable<Item> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(i =>
                EF.Functions.ILike(i.Identifier ?? "", pattern));
        }
    }
}
