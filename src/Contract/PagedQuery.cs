using System.ComponentModel;

namespace FireInvent.Contract
{
    public record PagedQuery
    {
        [Description("Page number (starting from 1)")]
        [DefaultValue(1)]
        public int Page { get; init; } = 1;

        [Description("Page size (number of items per page)")]
        [DefaultValue(ModelConstants.DefaultPageSize)]
        public int PageSize { get; init; } = ModelConstants.DefaultPageSize;
        
        [Description("Search term for filtering results. Case insensitive.")]
        public string? SearchTerm { get; init; }
    }
}
