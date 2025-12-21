using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVisitService
{
    Task<VisitModel> CreateVisitAsync(CreateOrUpdateVisitModel model);
    Task<bool> DeleteVisitAsync(Guid id);
    Task<List<VisitModel>> GetAllVisitsAsync();
    Task<VisitModel?> GetVisitByIdAsync(Guid id);
    Task<List<VisitModel>> GetVisitsForAppointmentAsync(Guid appointmentId);
    Task<bool> UpdateVisitAsync(Guid id, CreateOrUpdateVisitModel model);
}
