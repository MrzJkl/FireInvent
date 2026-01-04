using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class PersonQueryExtensions
    {
        public static IQueryable<Person> ApplySearch(
            this IQueryable<Person> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(p =>
                EF.Functions.ILike(p.FirstName, pattern) ||
                EF.Functions.ILike(p.LastName, pattern) ||
                EF.Functions.ILike(p.EMail ?? "", pattern) ||
                EF.Functions.ILike(p.ExternalId ?? "", pattern) ||
                EF.Functions.ILike(p.Remarks ?? "", pattern)
            );
        }
    }
}
