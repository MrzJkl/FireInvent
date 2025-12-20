using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IAppointmentService
{
    Task<AppointmentModel> CreateAppointmentAsync(CreateOrUpdateAppointmentModel model);
    Task<bool> DeleteAppointmentAsync(Guid id);
    Task<List<AppointmentModel>> GetAllAppointmentsAsync();
    Task<AppointmentModel?> GetAppointmentByIdAsync(Guid id);
    Task<bool> UpdateAppointmentAsync(Guid id, CreateOrUpdateAppointmentModel model);
}
