using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record CreateOrUpdateVisitItemModel
    {
        [Required]
        public Guid VisitId { get; init; }

        [Required]
        public Guid ProductId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; init; }
    }
}
