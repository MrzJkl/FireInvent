using AutoMapper;
using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class PersonService(GearDbContext context, IMapper mapper)
{
    public async Task<PersonModel> CreatePersonAsync(CreatePersonModel model)
    {
        var exists = await context.Persons.AnyAsync(p =>
            (p.FirstName == model.FirstName && p.LastName == model.LastName) ||
            (model.ExternalId != null && p.ExternalId == model.ExternalId));

        if (exists)
            throw new ConflictException("A person with the same name or external ID already exists.");

        var person = mapper.Map<Person>(model);
        person.Id = Guid.NewGuid();

        context.Persons.Add(person);
        await context.SaveChangesAsync();

        return mapper.Map<PersonModel>(person);
    }

    public async Task<List<PersonModel>> GetAllPersonsAsync()
    {
        var persons = await context.Persons
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<PersonModel>>(persons);
    }

    public async Task<PersonModel?> GetPersonByIdAsync(Guid id)
    {
        var person = await context.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return person is null ? null : mapper.Map<PersonModel>(person);
    }

    public async Task<bool> UpdatePersonAsync(PersonModel model)
    {
        var person = await context.Persons.FindAsync(model.Id);
        if (person is null)
            return false;

        var nameExists = await context.Persons.AnyAsync(p =>
            p.Id != model.Id &&
            p.FirstName == model.FirstName &&
            p.LastName == model.LastName);

        if (nameExists)
            throw new ConflictException("Another person with the same name already exists.");

        if (!string.IsNullOrWhiteSpace(model.ExternalId))
        {
            var extIdExists = await context.Persons.AnyAsync(p =>
                p.Id != model.Id &&
                p.ExternalId == model.ExternalId);

            if (extIdExists)
                throw new ConflictException("Another person with the same external ID already exists.");
        }

        mapper.Map(model, person);

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

        return mapper.Map<List<PersonModel>>(persons);
    }
}