using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IDepartmentService
    {
        Task<DepartmentModel> CreateDepartmentAsync(CreateDepartmentModel model);
        Task<bool> DeleteDepartmentAsync(Guid id);
        Task<List<DepartmentModel>> GetAllDepartmentsAsync();
        Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id);
        Task<bool> UpdateDepartmentAsync(DepartmentModel model);
    }
}