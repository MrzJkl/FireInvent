using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services
{
    public class ManufacturerService(AppDbContext context, ManufacturerMapper mapper) : IManufacturerService
    {
        public async Task<ManufacturerModel> CreateManufacturerAsync(CreateOrUpdateManufacturerModel model, CancellationToken cancellationToken = default)
        {
            var exists = await context.Manufacturers
                .AnyAsync(p => p.Name == model.Name, cancellationToken);

            if (exists)
                throw new ConflictException("A manufacturer with the same name already exists.");

            var manufacturer = mapper.MapCreateOrUpdateManufacturerModelToManufacturer(model);

            await context.Manufacturers.AddAsync(manufacturer, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.MapManufacturerToManufacturerModel(manufacturer);
        }

        public async Task<PagedResult<ManufacturerModel>> GetAllManufacturersAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
        {
            var query = context.Manufacturers
                .OrderBy(m => m.Name)
                .AsNoTracking();

            query = query.ApplySearch(pagedQuery.SearchTerm);

            var projected = mapper.ProjectManufacturersToManufacturerModels(query);

            return await projected.ToPagedResultAsync(
                pagedQuery.Page,
                pagedQuery.PageSize,
                cancellationToken);
        }

        public async Task<ManufacturerModel?> GetManufacturerByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var manufacturer = await context.Manufacturers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return manufacturer is null ? null : mapper.MapManufacturerToManufacturerModel(manufacturer);
        }

        public async Task<bool> UpdateManufacturerAsync(Guid id, CreateOrUpdateManufacturerModel model, CancellationToken cancellationToken = default)
        {
            var manufacturer = await context.Manufacturers.FindAsync(id, cancellationToken);
            if (manufacturer is null)
                return false;

            var nameExists = await context.Manufacturers.AnyAsync(p =>
                p.Id != id && p.Name == model.Name, cancellationToken);

            if (nameExists)
                throw new ConflictException("Another manufacturer with the same name already exists.");

            mapper.MapCreateOrUpdateManufacturerModelToManufacturer(model, manufacturer);

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteManufacturerAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var manufacturer = await context.Manufacturers.FindAsync(id, cancellationToken);
            if (manufacturer is null)
                return false;

            context.Manufacturers.Remove(manufacturer);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
