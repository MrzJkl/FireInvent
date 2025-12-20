using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record VisitItemModel : CreateOrUpdateVisitItemModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public ProductModel Product { get; init; } = null!;
    }
}
