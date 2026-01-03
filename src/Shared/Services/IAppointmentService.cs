using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IAppointmentService
{
    Task<AppointmentModel> CreateAppointmentAsync(CreateOrUpdateAppointmentModel model);
    Task<bool> DeleteAppointmentAsync(Guid id);
    Task<PagedResult<AppointmentModel>> GetAllAppointmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<AppointmentModel?> GetAppointmentByIdAsync(Guid id);
    Task<bool> UpdateAppointmentAsync(Guid id, CreateOrUpdateAppointmentModel model);
}
