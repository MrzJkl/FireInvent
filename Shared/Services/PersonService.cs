using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services
{
    public class PersonService(GearDbContext context)
    {
        public async Task<PersonModel> CreatePersonAsync(PersonModel model)
        {
            var exists = await context.Persons.AnyAsync(p =>
                (p.FirstName == model.FirstName && p.LastName == model.LastName) ||
                (model.ExternalId != null && p.ExternalId == model.ExternalId));

            if (exists)
                throw new ConflictException("A person with the same name or external ID already exists.");

            var person = new Person
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Remarks = model.Remarks,
                ContactInfo = model.ContactInfo,
                ExternalId = model.ExternalId
            };

            context.Persons.Add(person);
            await context.SaveChangesAsync();

            return new PersonModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Remarks = person.Remarks,
                ContactInfo = person.ContactInfo,
                ExternalId = person.ExternalId
            };
        }

        public async Task<List<PersonModel>> GetAllPersonsAsync()
        {
            return await context.Persons
                .AsNoTracking()
                .Select(p => new PersonModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Remarks = p.Remarks,
                    ContactInfo = p.ContactInfo,
                    ExternalId = p.ExternalId
                })
                .ToListAsync();
        }

        public async Task<PersonModel?> GetPersonByIdAsync(Guid id)
        {
            var person = await context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person is null)
                return null;

            return new PersonModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Remarks = person.Remarks,
                ContactInfo = person.ContactInfo,
                ExternalId = person.ExternalId
            };
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

            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.Remarks = model.Remarks;
            person.ContactInfo = model.ContactInfo;
            person.ExternalId = model.ExternalId;

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
    }
}
