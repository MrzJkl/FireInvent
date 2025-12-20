using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class MaintenanceService(AppDbContext context, IUserService userService, MaintenanceMapper mapper) : IMaintenanceService
{
    public async Task<MaintenanceModel> CreateMaintenanceAsync(CreateOrUpdateMaintenanceModel model)
    {
        _ = await context.Items.FindAsync(model.ItemId) ?? throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");
        _ = await context.MaintenanceTypes.FindAsync(model.TypeId) ?? throw new BadRequestException($"MaintenanceType with ID '{model.TypeId}' does not exist.");

        if (model.PerformedById.HasValue)
        {
            _ = await userService.GetUserByIdAsync(model.PerformedById.Value) ?? throw new BadRequestException($"User with ID '{model.PerformedById}' does not exist.");
        }

        var maintenance = mapper.MapCreateOrUpdateMaintenanceModelToMaintenance(model);

        await context.Maintenances.AddAsync(maintenance);
        await context.SaveChangesAsync();

        maintenance = await context.Maintenances
            .AsNoTracking()
            .SingleAsync(m => m.Id == maintenance.Id);

        return mapper.MapMaintenanceToMaintenanceModel(maintenance);
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

    public async Task<bool> UpdateMaintenanceAsync(Guid id, CreateOrUpdateMaintenanceModel model)
    {
        var entity = await context.Maintenances.FindAsync(id);
        if (entity is null)
            return false;

        _ = await context.Items.FindAsync(model.ItemId) ?? throw new BadRequestException($"Item with ID '{model.TypeId}' does not exist.");
        _ = await context.MaintenanceTypes.FindAsync(model.TypeId) ?? throw new BadRequestException($"MaintenanceType with ID '{model.TypeId}' does not exist.");

        if (model.PerformedById.HasValue)
        {
            _ = await userService.GetUserByIdAsync(model.PerformedById.Value) ?? throw new BadRequestException($"User with ID '{model.PerformedById}' does not exist.");
        }

        mapper.MapCreateOrUpdateMaintenanceModelToMaintenance(model, entity);

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
            throw new NotFoundException($"Item with ID {itemId} not found.");

        var maintenances = await context.Maintenances
            .Where(m => m.ItemId == itemId)
            .OrderByDescending(m => m.PerformedAt)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapMaintenancesToMaintenanceModels(maintenances);
    }
}