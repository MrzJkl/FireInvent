using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record CreateOrUpdateVisitModel
    {
        [Required]
        public Guid AppointmentId { get; init; }

        [Required]
        public Guid PersonId { get; init; }
    }
}
