using Microsoft.EntityFrameworkCore;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;

namespace FireInvent.Database.Extensions
{
    public static class QueryablePagingExtensions
    {
        public static async Task<PagedResult<TResult>> ToPagedResultAsync<TResult>(
            this IQueryable<TResult> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<TResult>
            {
                Items = items,
                TotalItems = total,
                Page = page,
                PageSize = pageSize,
            };

            if (result.Page > result.TotalPages)
            {
                throw new BadRequestException("Requested page exceeds total number of pages.");
            }

            return result;
        }
    }
}
