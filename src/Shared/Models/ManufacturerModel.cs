using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record ManufacturerModel : CreateOrUpdateManufacturerModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; init; }

        [Required]
        public Guid CreatedById { get; init; }

        public DateTimeOffset? ModifiedAt { get; init; }

        public Guid? ModifiedById { get; init; }
    }
}
