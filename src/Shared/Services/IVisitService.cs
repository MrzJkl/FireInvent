using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVisitService
{
    Task<VisitModel> CreateVisitAsync(CreateOrUpdateVisitModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteVisitAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VisitModel>> GetAllVisitsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<VisitModel?> GetVisitByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VisitModel>> GetVisitsForAppointmentAsync(Guid appointmentId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateVisitAsync(Guid id, CreateOrUpdateVisitModel model, CancellationToken cancellationToken = default);
}
