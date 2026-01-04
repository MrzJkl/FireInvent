using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions
{
    public static class AppointmentQueryExtensions
    {
        public static IQueryable<Appointment> ApplySearch(
            this IQueryable<Appointment> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            search = search.Trim();
            var pattern = $"%{search}%";

            return query.Where(a =>
                EF.Functions.ILike(a.Description ?? "", pattern) ||
                EF.Functions.ILike(a.Id.ToString() ?? "", pattern)
            );
        }
    }
}
