using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models
{
    public record MaintenanceModel
    {
        public Guid Id { get; init; }

        [Required]
        public Guid ItemId { get; init; }

        [Required]
        public DateTime Performed { get; init; }

        [Required]
        public MaintenanceType MaintenanceType { get; init; }

        [MaxLength(ModelConstants.MaxStringLengthLong)]
        public string? Remarks { get; init; }
    }
}
