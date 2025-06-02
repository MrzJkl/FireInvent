using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models
{
    public record ClothingProductModel
    {
        public Guid Id { get; init; }

        [Required]

        [MaxLength(ModelConstants.MaxStringLength)]
        public string Name { get; init; } = string.Empty;

        [Required]
        [MaxLength(ModelConstants.MaxStringLength)]
        public string Manufacturer { get; init; } = string.Empty;

        [MaxLength(ModelConstants.MaxStringLengthLong)]
        public string? Description { get; init; }

        [Required]
        public GearType Type { get; init; }
    }
}
