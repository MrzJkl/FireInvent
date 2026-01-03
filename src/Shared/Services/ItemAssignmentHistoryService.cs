using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;
using FireInvent.Shared.Services.Keycloak;

namespace FireInvent.Shared.Services;

public class ItemAssignmentHistoryService(AppDbContext context, ItemAssignmentHistoryMapper mapper, IKeycloakUserService userService) : IItemAssignmentHistoryService
{
    public async Task<ItemAssignmentHistoryModel> CreateAssignmentAsync(CreateOrUpdateItemAssignmentHistoryModel model, CancellationToken cancellationToken = default)
    {
        ValidateAssignmentTarget(model);

        if (!await context.Items.AnyAsync(i => i.Id == model.ItemId, cancellationToken))
            throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");

        if (model.PersonId.HasValue && !await context.Persons.AnyAsync(p => p.Id == model.PersonId, cancellationToken))
            throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        if (model.StorageLocationId.HasValue && !await context.StorageLocations.AnyAsync(s => s.Id == model.StorageLocationId, cancellationToken))
            throw new BadRequestException($"StorageLocation with ID '{model.StorageLocationId}' does not exist.");

        _ = await userService.GetUserByIdAsync(model.AssignedById, cancellationToken) ?? throw new BadRequestException($"User with ID '{model.AssignedById}' does not exist.");

        bool overlapExists = await context.ItemAssignmentHistories
            .AnyAsync(a =>
                a.ItemId == model.ItemId &&
                (
                    (model.AssignedUntil == null || a.AssignedFrom <= model.AssignedUntil) &&
                    (a.AssignedUntil == null || a.AssignedUntil >= model.AssignedFrom)
                ), cancellationToken);

        if (overlapExists)
            throw new ConflictException("An overlapping assignment already exists for this item.");

        var assignment = mapper.MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(model);

        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync(cancellationToken);

        assignment = await context.ItemAssignmentHistories
            .AsNoTracking()
            .SingleAsync(a => a.Id == assignment.Id, cancellationToken);

        return mapper.MapItemAssignmentHistoryToItemAssignmentHistoryModel(assignment);
    }

    public async Task<PagedResult<ItemAssignmentHistoryModel>> GetAllAssignmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.ItemAssignmentHistories
            .OrderByDescending(a => a.AssignedFrom)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectItemAssignmentHistorysToItemAssignmentHistoryModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var itemExists = await context.Items.AnyAsync(i => i.Id == itemId);
        if (!itemExists)
            throw new NotFoundException($"Item with ID {itemId} not found.");

        var query = context.ItemAssignmentHistories
            .Where(a => a.ItemId == itemId)
            .OrderByDescending(a => a.AssignedFrom)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectItemAssignmentHistorysToItemAssignmentHistoryModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ItemAssignmentHistoryModel>> GetAssignmentsForPersonAsync(Guid personId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var personExists = await context.Persons.AnyAsync(p => p.Id == personId);
        if (!personExists)
            throw new NotFoundException($"Person with ID {personId} not found.");

        var query = context.ItemAssignmentHistories
            .Where(a => a.PersonId == personId)
            .OrderByDescending(a => a.AssignedFrom)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectItemAssignmentHistorysToItemAssignmentHistoryModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<ItemAssignmentHistoryModel>> GetAssignmentsForStorageLocationAsync(Guid storageLocationId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var locationExists = await context.StorageLocations.AnyAsync(s => s.Id == storageLocationId);
        if (!locationExists)
            throw new NotFoundException($"StorageLocation with ID {storageLocationId} not found.");

        var query = context.ItemAssignmentHistories
            .Where(a => a.StorageLocationId == storageLocationId)
            .OrderByDescending(a => a.AssignedFrom)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectItemAssignmentHistorysToItemAssignmentHistoryModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<ItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ItemAssignmentHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return entity is null ? null : mapper.MapItemAssignmentHistoryToItemAssignmentHistoryModel(entity);
    }

    public async Task<bool> UpdateAssignmentAsync(Guid id, CreateOrUpdateItemAssignmentHistoryModel model, CancellationToken cancellationToken = default)
    {
        var entity = await context.ItemAssignmentHistories
            .FindAsync(id, cancellationToken);
        if (entity is null)
            return false;

        ValidateAssignmentTarget(model);

        if (!await context.Items.AnyAsync(i => i.Id == model.ItemId, cancellationToken))
            throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");

        if (model.PersonId.HasValue && !await context.Persons.AnyAsync(p => p.Id == model.PersonId, cancellationToken))
            throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        if (model.StorageLocationId.HasValue && !await context.StorageLocations.AnyAsync(s => s.Id == model.StorageLocationId, cancellationToken))
            throw new BadRequestException($"StorageLocation with ID '{model.StorageLocationId}' does not exist.");

        _ = await userService.GetUserByIdAsync(model.AssignedById, cancellationToken) ?? throw new BadRequestException($"User with ID '{model.AssignedById}' does not exist.");

        bool overlapExists = await context.ItemAssignmentHistories
            .AnyAsync(a =>
                a.Id != id &&
                a.ItemId == model.ItemId &&
                (
                    (model.AssignedUntil == null || a.AssignedFrom <= model.AssignedUntil) &&
                    (a.AssignedUntil == null || a.AssignedUntil >= model.AssignedFrom)
                ), cancellationToken);

        if (overlapExists)
            throw new ConflictException("An overlapping assignment already exists for this item.");

        mapper.MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(model, entity);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.ItemAssignmentHistories.FindAsync(id, cancellationToken);
        if (entity is null)
            return false;

        context.ItemAssignmentHistories.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ValidateAssignmentTarget(CreateOrUpdateItemAssignmentHistoryModel model)
    {
        bool hasPersonId = model.PersonId.HasValue;
        bool hasStorageLocationId = model.StorageLocationId.HasValue;

        if (!hasPersonId && !hasStorageLocationId)
            throw new BadRequestException("Either PersonId or StorageLocationId must be set.");

        if (hasPersonId && hasStorageLocationId)
            throw new BadRequestException("An item can only be assigned to either a Person or a StorageLocation, not both.");
    }
}
