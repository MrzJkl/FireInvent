namespace FireInvent.Shared.Models;

public record AppointmentModel : CreateOrUpdateAppointmentModel
{
    public Guid Id { get; init; }
}
