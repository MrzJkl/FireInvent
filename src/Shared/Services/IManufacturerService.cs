using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IManufacturerService
    {
        Task<ManufacturerModel> CreateManufacturerAsync(CreateOrUpdateManufacturerModel model, CancellationToken cancellationToken = default);
        Task<bool> DeleteManufacturerAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<ManufacturerModel>> GetAllManufacturersAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
        Task<ManufacturerModel?> GetManufacturerByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> UpdateManufacturerAsync(Guid id, CreateOrUpdateManufacturerModel model, CancellationToken cancellationToken = default);
    }
}