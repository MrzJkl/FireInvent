using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services.Telemetry;
using Microsoft.EntityFrameworkCore;
using FireInvent.Shared.Services.Keycloak;

namespace FireInvent.Shared.Services;

public class MaintenanceService(AppDbContext context, IKeycloakUserService userService, MaintenanceMapper mapper, FireInventTelemetry telemetry) : IMaintenanceService
{
    public async Task<MaintenanceModel> CreateMaintenanceAsync(CreateOrUpdateMaintenanceModel model, CancellationToken cancellationToken = default)
    {
        using var activity = telemetry.StartActivity("MaintenanceService.CreateMaintenance");
        activity?.SetTag("item.id", model.ItemId);
        activity?.SetTag("maintenance.type", model.TypeId);

        _ = await context.Items.FindAsync(model.ItemId, cancellationToken) ?? throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");
        _ = await context.MaintenanceTypes.FindAsync(model.TypeId, cancellationToken) ?? throw new BadRequestException($"MaintenanceType with ID '{model.TypeId}' does not exist.");
        _ = await userService.GetUserByIdAsync(model.PerformedById, cancellationToken) ?? throw new BadRequestException($"User with ID '{model.PerformedById}' does not exist.");

        var maintenance = mapper.MapCreateOrUpdateMaintenanceModelToMaintenance(model);

        await context.Maintenances.AddAsync(maintenance, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        maintenance = await context.Maintenances
            .AsNoTracking()
            .SingleAsync(m => m.Id == maintenance.Id, cancellationToken);

        // Record telemetry
        telemetry.MaintenanceRecordsCounter.Add(1,
            new KeyValuePair<string, object?>("item.id", model.ItemId),
            new KeyValuePair<string, object?>("maintenance.type", model.TypeId));

        activity?.SetTag("maintenance.id", maintenance.Id);

        return mapper.MapMaintenanceToMaintenanceModel(maintenance);
    }

    public async Task<PagedResult<MaintenanceModel>> GetAllMaintenancesAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Maintenances
            .OrderByDescending(m => m.PerformedAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectMaintenancesToMaintenanceModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<MaintenanceModel?> GetMaintenanceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var maintenance = await context.Maintenances
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return maintenance is null ? null : mapper.MapMaintenanceToMaintenanceModel(maintenance);
    }

    public async Task<bool> UpdateMaintenanceAsync(Guid id, CreateOrUpdateMaintenanceModel model, CancellationToken cancellationToken = default)
    {
        var entity = await context.Maintenances.FindAsync(id, cancellationToken);
        if (entity is null)
            return false;

        _ = await context.Items.FindAsync(model.ItemId, cancellationToken) ?? throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");
        _ = await context.MaintenanceTypes.FindAsync(model.TypeId, cancellationToken) ?? throw new BadRequestException($"MaintenanceType with ID '{model.TypeId}' does not exist.");
        _ = await userService.GetUserByIdAsync(model.PerformedById, cancellationToken) ?? throw new BadRequestException($"User with ID '{model.PerformedById}' does not exist.");

        mapper.MapCreateOrUpdateMaintenanceModelToMaintenance(model, entity);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteMaintenanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Maintenances.FindAsync(id, cancellationToken);
        if (entity is null)
            return false;

        context.Maintenances.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<MaintenanceModel>> GetMaintenancesForItemAsync(Guid itemId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var itemExists = await context.Items.AnyAsync(i => i.Id == itemId);
        if (!itemExists)
            throw new NotFoundException($"Item with ID {itemId} not found.");

        var query = context.Maintenances
            .Where(m => m.ItemId == itemId)
            .OrderByDescending(m => m.PerformedAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectMaintenancesToMaintenanceModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }
}
