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
        public async Task<MaintenanceTypeModel> CreateMaintenanceTypeAsync(CreateOrUpdateMaintenanceTypeModel model)
        {
            var exists = await context.MaintenanceTypes
                .AnyAsync(p => p.Name == model.Name);

            if (exists)
                throw new ConflictException("A maintenanceType with the same name already exists.");

            var maintenanceType = mapper.MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(model);

            await context.MaintenanceTypes.AddAsync(maintenanceType);
            await context.SaveChangesAsync();

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

        public async Task<MaintenanceTypeModel?> GetMaintenanceTypeByIdAsync(Guid id)
        {
            var maintenanceType = await context.MaintenanceTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return maintenanceType is null ? null : mapper.MapMaintenanceTypeToMaintenanceTypeModel(maintenanceType);
        }

        public async Task<bool> UpdateMaintenanceTypeAsync(Guid id, CreateOrUpdateMaintenanceTypeModel model)
        {
            var maintenanceType = await context.MaintenanceTypes.FindAsync(id);
            if (maintenanceType is null)
                return false;

            var nameExists = await context.MaintenanceTypes.AnyAsync(p =>
                p.Id != id && p.Name == model.Name);

            if (nameExists)
                throw new ConflictException("Another maintenanceType with the same name already exists.");

            mapper.MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(model, maintenanceType);

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMaintenanceTypeAsync(Guid id)
        {
            var maintenanceType = await context.MaintenanceTypes.FindAsync(id);
            if (maintenanceType is null)
                return false;

            context.MaintenanceTypes.Remove(maintenanceType);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
