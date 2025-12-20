using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IManufacturerService
    {
        Task<ManufacturerModel> CreateManufacturerAsync(CreateOrUpdateManufacturerModel model);
        Task<bool> DeleteManufacturerAsync(Guid id);
        Task<List<ManufacturerModel>> GetAllManufacturersAsync();
        Task<ManufacturerModel?> GetManufacturerByIdAsync(Guid id);
        Task<bool> UpdateManufacturerAsync(Guid id, CreateOrUpdateManufacturerModel model);
    }
}