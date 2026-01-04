using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IAppointmentService
{
    Task<AppointmentModel> CreateAppointmentAsync(CreateOrUpdateAppointmentModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<AppointmentModel>> GetAllAppointmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<AppointmentModel?> GetAppointmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAppointmentAsync(Guid id, CreateOrUpdateAppointmentModel model, CancellationToken cancellationToken = default);
}
