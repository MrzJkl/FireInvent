using FireInvent.Contract.Exceptions;
using FireInvent.Database;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services
{
    public class ManufacturerService(AppDbContext context, ManufacturerMapper mapper) : IManufacturerService
    {
        public async Task<ManufacturerModel> CreateManufacturerAsync(CreateOrUpdateManufacturerModel model)
        {
            var exists = await context.Manufacturers
                .AnyAsync(p => p.Name == model.Name);

            if (exists)
                throw new ConflictException("A manufacturer with the same name already exists.");

            var manufacturer = mapper.MapCreateOrUpdateManufacturerModelToManufacturer(model);

            await context.Manufacturers.AddAsync(manufacturer);
            await context.SaveChangesAsync();

            return mapper.MapManufacturerToManufacturerModel(manufacturer);
        }

        public async Task<List<ManufacturerModel>> GetAllManufacturersAsync()
        {
            var manufacturers = await context.Manufacturers
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            return mapper.MapManufacturersToManufacturerModels(manufacturers);
        }

        public async Task<ManufacturerModel?> GetManufacturerByIdAsync(Guid id)
        {
            var manufacturer = await context.Manufacturers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return manufacturer is null ? null : mapper.MapManufacturerToManufacturerModel(manufacturer);
        }

        public async Task<bool> UpdateManufacturerAsync(Guid id, CreateOrUpdateManufacturerModel model)
        {
            var manufacturer = await context.Manufacturers.FindAsync(id);
            if (manufacturer is null)
                return false;

            var nameExists = await context.Manufacturers.AnyAsync(p =>
                p.Id != id && p.Name == model.Name);

            if (nameExists)
                throw new ConflictException("Another manufacturer with the same name already exists.");

            mapper.MapCreateOrUpdateManufacturerModelToManufacturer(model, manufacturer);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteManufacturerAsync(Guid id)
        {
            var manufacturer = await context.Manufacturers.FindAsync(id);
            if (manufacturer is null)
                return false;

            context.Manufacturers.Remove(manufacturer);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
