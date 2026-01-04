using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class DepartmentQueryExtensions
    {
        public static IQueryable<Department> ApplySearch(
            this IQueryable<Department> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(d =>
                EF.Functions.ILike(d.Name, pattern) ||
                EF.Functions.ILike(d.Description ?? "", pattern) ||
                EF.Functions.ILike(d.Id.ToString() ?? "", pattern)
            );
        }
    }
}
