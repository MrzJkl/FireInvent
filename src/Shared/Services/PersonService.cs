using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class PersonService(AppDbContext context, PersonMapper mapper) : IPersonService
{
    public async Task<PersonModel> CreatePersonAsync(CreateOrUpdatePersonModel model, CancellationToken cancellationToken = default)
    {
        var exists = await context.Persons.AnyAsync(p =>
            (p.FirstName == model.FirstName && p.LastName == model.LastName) ||
            (model.ExternalId != null && p.ExternalId == model.ExternalId), cancellationToken);

        if (exists)
            throw new ConflictException("A person with the same name or external ID already exists.");

        var person = mapper.MapCreateOrUpdatePersonModelToPerson(model);

        await context.Persons.AddAsync(person, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.MapPersonToPersonModel(person);
    }

    public async Task<PagedResult<PersonModel>> GetAllPersonsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Persons
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectPersonsToPersonModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<PersonModel?> GetPersonByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var person = await context.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return person is null ? null : mapper.MapPersonToPersonModel(person);
    }

    public async Task<bool> UpdatePersonAsync(Guid id, CreateOrUpdatePersonModel model, CancellationToken cancellationToken = default)
    {
        var person = await context.Persons
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (person is null)
            return false;

        var nameExists = await context.Persons.AnyAsync(p =>
            p.Id != id &&
            p.FirstName == model.FirstName &&
            p.LastName == model.LastName, cancellationToken);

        if (nameExists)
            throw new ConflictException("Another person with the same name already exists.");

        if (!string.IsNullOrWhiteSpace(model.ExternalId))
        {
            var extIdExists = await context.Persons.AnyAsync(p =>
                p.Id != id &&
                p.ExternalId == model.ExternalId, cancellationToken);

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
                .ToListAsync(cancellationToken);

            person.Departments = departments;
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeletePersonAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var person = await context.Persons.FindAsync([id], cancellationToken);
        if (person is null)
            return false;

        context.Persons.Remove(person);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<PersonModel>> GetPersonsForDepartmentAsync(Guid departmentId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var departmentExists = await context.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
        if (!departmentExists)
            throw new NotFoundException($"Department with ID {departmentId} not found.");

        var query = context.Persons
            .Where(p => p.Departments.Any(d => d.Id == departmentId))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectPersonsToPersonModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }
}