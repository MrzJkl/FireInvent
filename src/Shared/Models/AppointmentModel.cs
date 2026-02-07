namespace FireInvent.Shared.Models;

public record AppointmentModel : CreateOrUpdateAppointmentModel
{
    public Guid Id { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public UserModel? CreatedBy { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public UserModel? ModifiedBy { get; init; }
}
