using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVisitService
{
    Task<VisitModel> CreateVisitAsync(CreateOrUpdateVisitModel model);
    Task<bool> DeleteVisitAsync(Guid id);
    Task<PagedResult<VisitModel>> GetAllVisitsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<VisitModel?> GetVisitByIdAsync(Guid id);
    Task<PagedResult<VisitModel>> GetVisitsForAppointmentAsync(Guid appointmentId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateVisitAsync(Guid id, CreateOrUpdateVisitModel model);
}
