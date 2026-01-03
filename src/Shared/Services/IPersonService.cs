using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IPersonService
{
    Task<PersonModel> CreatePersonAsync(CreateOrUpdatePersonModel model, CancellationToken cancellationToken = default);
    Task<bool> DeletePersonAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<PersonModel>> GetAllPersonsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PersonModel?> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<PersonModel>> GetPersonsForDepartmentAsync(Guid departmentId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdatePersonAsync(Guid id, CreateOrUpdatePersonModel model, CancellationToken cancellationToken = default);
}