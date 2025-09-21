using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IPersonService
{
    Task<PersonModel> CreatePersonAsync(CreatePersonModel model);
    Task<bool> DeletePersonAsync(Guid id);
    Task<List<PersonModel>> GetAllPersonsAsync();
    Task<PersonModel?> GetPersonByIdAsync(Guid id);
    Task<List<PersonModel>> GetPersonsForDepartmentAsync(Guid departmentId);
    Task<bool> UpdatePersonAsync(PersonModel model);
}