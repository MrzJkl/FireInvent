using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class MaintenanceService(AppDbContext context, IUserService userService, MaintenanceMapper mapper) : IMaintenanceService
{
    public async Task<MaintenanceModel> CreateMaintenanceAsync(CreateMaintenanceModel model)
    {
        _ = await context.Items.FindAsync(model.ItemId) ?? throw new BadRequestException($"ClothingItem with ID '{model.ItemId}' does not exist.");

        if (model.PerformedById.HasValue)
        {
            _ = await userService.GetUserByIdAsync(model.PerformedById.Value) ?? throw new BadRequestException($"User with ID '{model.PerformedById}' does not exist.");
        }

        var entity = mapper.MapCreateMaintenanceModelToMaintenance(model);

        context.Maintenances.Add(entity);
        await context.SaveChangesAsync();

        return mapper.MapMaintenanceToMaintenanceModel(entity);
    }

    public async Task<List<MaintenanceModel>> GetAllMaintenancesAsync()
    {
        var entities = await context.Maintenances
            .AsNoTracking()
            .OrderByDescending(v => v.PerformedAt)
            .ToListAsync();

        return mapper.MapMaintenancesToMaintenanceModels(entities);
    }

    public async Task<MaintenanceModel?> GetMaintenanceByIdAsync(Guid id)
    {
        var maintenance = await context.Maintenances
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return maintenance is null ? null : mapper.MapMaintenanceToMaintenanceModel(maintenance);
    }

    public async Task<bool> UpdateMaintenanceAsync(MaintenanceModel model)
    {
        var entity = await context.Maintenances.FindAsync(model.Id);
        if (entity is null)
            return false;

        var itemExists = await context.Items.AnyAsync(i => i.Id == model.ItemId);
        if (!itemExists)
            throw new BadRequestException($"ClothingItem with ID '{model.ItemId}' does not exist.");

        if (model.PerformedById.HasValue)
        {
            _ = await userService.GetUserByIdAsync(model.PerformedById.Value) ?? throw new BadRequestException($"User with ID '{model.PerformedById}' does not exist.");
        }

        mapper.MapMaintenanceModelToMaintenance(model, entity);

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
        var itemExists = await context.Items.AnyAsync(i => i.Id == itemId);
        if (!itemExists)
            throw new NotFoundException($"ClothingItem with ID {itemId} not found.");

        var maintenances = await context.Maintenances
            .Where(m => m.ItemId == itemId)
            .OrderByDescending(m => m.PerformedAt)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapMaintenancesToMaintenanceModels(maintenances);
    }
}