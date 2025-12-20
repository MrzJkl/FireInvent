using FireInvent.Database;
using FireInvent.Contract.Exceptions;
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

        public async Task<List<MaintenanceTypeModel>> GetAllMaintenanceTypesAsync()
        {
            var maintenanceTypes = await context.MaintenanceTypes
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            return mapper.MapMaintenanceTypesToMaintenanceTypeModels(maintenanceTypes);
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
