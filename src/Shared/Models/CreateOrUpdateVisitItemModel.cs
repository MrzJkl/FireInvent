using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record CreateOrUpdateVisitItemModel
    {
        [Required]
        public Guid VisitId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
