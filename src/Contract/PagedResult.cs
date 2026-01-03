using System.ComponentModel;

namespace FireInvent.Contract
{
    public record PagedResult<TResult>
    {
        [Description("Items of the current page.")]
        public IReadOnlyList<TResult> Items { get; init; } = [];

        [Description("Total number of items existing.")]
        public int TotalItems { get; init; }

        [Description("Current page number (starting from 1).")]
        public int Page { get; init; }

        [Description("Number of items per page.")]
        [DefaultValue(ModelConstants.DefaultPageSize)]
        public int PageSize { get; init; } = ModelConstants.DefaultPageSize;

        [Description("Total number of pages available with the given page size.")]
        public int TotalPages =>
            PageSize <= 0
                ? 0
                : (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
