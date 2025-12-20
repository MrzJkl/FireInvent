using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class AppointmentMapper : BaseMapper
{
    public partial AppointmentModel MapAppointmentToAppointmentModel(Appointment appointment);

    [MapValue(nameof(Appointment.Id), Use = nameof(NewGuid))]
    public partial Appointment MapCreateOrUpdateAppointmentModelToAppointment(CreateOrUpdateAppointmentModel createAppointmentModel);

    public partial List<AppointmentModel> MapAppointmentsToAppointmentModels(List<Appointment> appointments);

    public partial void MapCreateOrUpdateAppointmentModelToAppointment(CreateOrUpdateAppointmentModel source, Appointment target);
}
