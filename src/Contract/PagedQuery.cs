using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Contract
{
    public record PagedQuery
    {
        [Description("Page number (starting from 1)")]
        [DefaultValue(1)]
        [Range(1, int.MaxValue)]
        [FromQuery]
        public int Page { get; init; } = 1;

        [Description("Page size (number of items per page)")]
        [DefaultValue(ModelConstants.DefaultPageSize)]
        [FromQuery]
        [Range(1, int.MaxValue)]
        public int PageSize { get; init; } = ModelConstants.DefaultPageSize;
        
        [Description("Search term for filtering results. Case insensitive.")]
        [FromQuery]
        [MaxLength(ModelConstants.MaxStringLength)]
        public string? SearchTerm { get; init; }
    }
}
