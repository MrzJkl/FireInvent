using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services
{
    public class MaintenanceTypeService(AppDbContext context, MaintenanceTypeMapper mapper) : IMaintenanceTypeService
    {
        public async Task<MaintenanceTypeModel> CreateMaintenanceTypeAsync(CreateOrUpdateMaintenanceTypeModel model, CancellationToken cancellationToken = default)
        {
            var exists = await context.MaintenanceTypes
                .AnyAsync(p => p.Name == model.Name, cancellationToken);

            if (exists)
                throw new ConflictException("A maintenanceType with the same name already exists.");

            var maintenanceType = mapper.MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(model);

            await context.MaintenanceTypes.AddAsync(maintenanceType, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.MapMaintenanceTypeToMaintenanceTypeModel(maintenanceType);
        }

        public async Task<PagedResult<MaintenanceTypeModel>> GetAllMaintenanceTypesAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
        {
            var query = context.MaintenanceTypes
                .OrderBy(mt => mt.Name)
                .AsNoTracking();

            query = query.ApplySearch(pagedQuery.SearchTerm);

            var projected = mapper.ProjectMaintenanceTypesToMaintenanceTypeModels(query);

            return await projected.ToPagedResultAsync(
                pagedQuery.Page,
                pagedQuery.PageSize,
                cancellationToken);
        }

        public async Task<MaintenanceTypeModel?> GetMaintenanceTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var maintenanceType = await context.MaintenanceTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            return maintenanceType is null ? null : mapper.MapMaintenanceTypeToMaintenanceTypeModel(maintenanceType);
        }

        public async Task<bool> UpdateMaintenanceTypeAsync(Guid id, CreateOrUpdateMaintenanceTypeModel model, CancellationToken cancellationToken = default)
        {
            var maintenanceType = await context.MaintenanceTypes.FindAsync(id, cancellationToken);
            if (maintenanceType is null)
                return false;

            var nameExists = await context.MaintenanceTypes.AnyAsync(p =>
                p.Id != id && p.Name == model.Name, cancellationToken);

            if (nameExists)
                throw new ConflictException("Another maintenanceType with the same name already exists.");

            mapper.MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(model, maintenanceType);

            await context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteMaintenanceTypeAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var maintenanceType = await context.MaintenanceTypes.FindAsync(id, cancellationToken);
            if (maintenanceType is null)
                return false;

            context.MaintenanceTypes.Remove(maintenanceType);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
