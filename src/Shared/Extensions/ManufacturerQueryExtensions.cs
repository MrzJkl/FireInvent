using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class ManufacturerQueryExtensions
    {
        public static IQueryable<Manufacturer> ApplySearch(
            this IQueryable<Manufacturer> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(m =>
                EF.Functions.ILike(m.Name, pattern) ||
                EF.Functions.ILike(m.Description ?? "", pattern) ||
                EF.Functions.ILike(m.Street ?? "", pattern) ||
                EF.Functions.ILike(m.City ?? "", pattern) ||
                EF.Functions.ILike(m.PostalCode ?? "", pattern) ||
                EF.Functions.ILike(m.Country ?? "", pattern) ||
                EF.Functions.ILike(m.Email ?? "", pattern) ||
                EF.Functions.ILike(m.PhoneNumber ?? "", pattern)
            );
        }
    }
}
