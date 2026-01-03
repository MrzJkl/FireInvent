using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IPersonService
{
    Task<PersonModel> CreatePersonAsync(CreateOrUpdatePersonModel model);
    Task<bool> DeletePersonAsync(Guid id);
    Task<PagedResult<PersonModel>> GetAllPersonsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PersonModel?> GetPersonByIdAsync(Guid id);
    Task<PagedResult<PersonModel>> GetPersonsForDepartmentAsync(Guid departmentId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdatePersonAsync(Guid id, CreateOrUpdatePersonModel model);
}