using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class PersonService(AppDbContext context, PersonMapper mapper) : IPersonService
{
    public async Task<PersonModel> CreatePersonAsync(CreateOrUpdatePersonModel model)
    {
        var exists = await context.Persons.AnyAsync(p =>
            (p.FirstName == model.FirstName && p.LastName == model.LastName) ||
            (model.ExternalId != null && p.ExternalId == model.ExternalId));

        if (exists)
            throw new ConflictException("A person with the same name or external ID already exists.");

        var person = mapper.MapCreateOrUpdatePersonModelToPerson(model);

        context.Persons.Add(person);
        await context.SaveChangesAsync();

        return mapper.MapPersonToPersonModel(person);
    }

    public async Task<List<PersonModel>> GetAllPersonsAsync()
    {
        var persons = await context.Persons
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapPersonsToPersonModels(persons);
    }

    public async Task<PersonModel?> GetPersonByIdAsync(Guid id)
    {
        var person = await context.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return person is null ? null : mapper.MapPersonToPersonModel(person);
    }

    public async Task<bool> UpdatePersonAsync(Guid id, CreateOrUpdatePersonModel model)
    {
        var person = await context.Persons
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person is null)
            return false;

        var nameExists = await context.Persons.AnyAsync(p =>
            p.Id != id &&
            p.FirstName == model.FirstName &&
            p.LastName == model.LastName);

        if (nameExists)
            throw new ConflictException("Another person with the same name already exists.");

        if (!string.IsNullOrWhiteSpace(model.ExternalId))
        {
            var extIdExists = await context.Persons.AnyAsync(p =>
                p.Id != id &&
                p.ExternalId == model.ExternalId);

            if (extIdExists)
                throw new ConflictException("Another person with the same external ID already exists.");
        }

        mapper.MapCreateOrUpdatePersonModelToPerson(model, person);

        person.Departments ??= [];

        if (model.DepartmentIds.Count == 0)
        {
            person.Departments.Clear();
        }
        else
        {
            var departments = await context.Departments
                .Where(d => model.DepartmentIds.Contains(d.Id))
                .ToListAsync();

            person.Departments = departments;
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePersonAsync(Guid id)
    {
        var person = await context.Persons.FindAsync(id);
        if (person is null)
            return false;

        context.Persons.Remove(person);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<PersonModel>> GetPersonsForDepartmentAsync(Guid departmentId)
    {
        var departmentExists = await context.Departments.AnyAsync(d => d.Id == departmentId);
        if (!departmentExists)
            throw new NotFoundException($"Department with ID {departmentId} not found.");

        var persons = await context.Persons
            .Where(p => p.Departments.Any(d => d.Id == departmentId))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapPersonsToPersonModels(persons);
    }
}