using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services
{
    public class MaintenanceService(GearDbContext context)
    {
        public async Task<MaintenanceModel> CreateMaintenanceAsync(MaintenanceModel model)
        {
            _ = await context.ClothingItems.FindAsync(model.ItemId) ?? throw new BadRequestException($"ClothingItem with ID '{model.ItemId}' does not exist.");

            var entity = new Maintenance
            {
                Id = Guid.NewGuid(),
                ItemId = model.ItemId,
                Performed = model.Performed,
                MaintenanceType = model.MaintenanceType,
                Remarks = model.Remarks
            };

            context.Maintenances.Add(entity);
            await context.SaveChangesAsync();

            return MapToModel(entity);
        }

        public async Task<List<MaintenanceModel>> GetAllMaintenancesAsync()
        {
            return await context.Maintenances
                .AsNoTracking()
                .Select(m => MapToModel(m))
                .ToListAsync();
        }

        public async Task<MaintenanceModel?> GetMaintenanceByIdAsync(Guid id)
        {
            var maintenance = await context.Maintenances
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return maintenance is null ? null : MapToModel(maintenance);
        }

        public async Task<bool> UpdateMaintenanceAsync(MaintenanceModel model)
        {
            var entity = await context.Maintenances.FindAsync(model.Id);
            if (entity is null)
                return false;

            var itemExists = await context.ClothingItems.AnyAsync(i => i.Id == model.ItemId);
            if (!itemExists)
                throw new BadRequestException($"ClothingItem with ID '{model.ItemId}' does not exist.");

            entity.ItemId = model.ItemId;
            entity.Performed = model.Performed;
            entity.MaintenanceType = model.MaintenanceType;
            entity.Remarks = model.Remarks;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMaintenanceAsync(Guid id)
        {
            var entity = await context.Maintenances.FindAsync(id);
            if (entity is null)
                return false;

            context.Maintenances.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MaintenanceModel>> GetMaintenancesForItemAsync(Guid itemId)
        {
            var itemExists = await context.ClothingItems.AnyAsync(i => i.Id == itemId);
            if (!itemExists)
                throw new NotFoundException($"ClothingItem with ID {itemId} not found.");

            return await context.Maintenances
                .Where(m => m.ItemId == itemId)
                .OrderByDescending(m => m.Performed)
                .AsNoTracking()
                .Select(m => new MaintenanceModel
                {
                    Id = m.Id,
                    ItemId = m.ItemId,
                    Performed = m.Performed,
                    MaintenanceType = m.MaintenanceType,
                    Remarks = m.Remarks
                })
                .ToListAsync();
        }

        private static MaintenanceModel MapToModel(Maintenance entity) => new()
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            Performed = entity.Performed,
            MaintenanceType = entity.MaintenanceType,
            Remarks = entity.Remarks
        };
    }
}
