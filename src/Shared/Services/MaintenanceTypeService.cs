using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services
{
    public class MaintenanceTypeService(AppDbContext context, MaintenanceTypeMapper mapper) : IMaintenanceTypeService
    {
        public async Task<MaintenanceTypeModel> CreateMaintenanceTypeAsync(CreateMaintenanceTypeModel model)
        {
            var exists = await context.MaintenanceTypes
                .AnyAsync(p => p.Name == model.Name);

            if (exists)
                throw new ConflictException("A maintenanceType with the same name already exists.");

            var maintenanceType = mapper.MapCreateMaintenanceTypeModelToMaintenanceType(model);

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

        public async Task<bool> UpdateMaintenanceTypeAsync(MaintenanceTypeModel model)
        {
            var maintenanceType = await context.MaintenanceTypes.FindAsync(model.Id);
            if (maintenanceType is null)
                return false;

            var nameExists = await context.MaintenanceTypes.AnyAsync(p =>
                p.Id != model.Id && p.Name == model.Name);

            if (nameExists)
                throw new ConflictException("Another maintenanceType with the same name already exists.");

            mapper.MapMaintenanceTypeModelToMaintenanceType(model, maintenanceType);

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
