using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record MaintenanceTypeModel : CreateMaintenanceTypeModel
    {
        [Required]
        public Guid Id { get; init; }
    }
}
