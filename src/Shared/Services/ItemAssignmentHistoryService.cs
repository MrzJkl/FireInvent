using FireInvent.Database;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ItemAssignmentHistoryService(AppDbContext context, ItemAssignmentHistoryMapper mapper, IKeycloakUserService userService) : IItemAssignmentHistoryService
{
    public async Task<ItemAssignmentHistoryModel> CreateAssignmentAsync(CreateOrUpdateItemAssignmentHistoryModel model)
    {
        if (!await context.Items.AnyAsync(i => i.Id == model.ItemId))
            throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");

        if (!await context.Persons.AnyAsync(p => p.Id == model.PersonId))
            throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        _ = await userService.GetUserByIdAsync(model.AssignedById) ?? throw new BadRequestException($"User with ID '{model.AssignedById}' does not exist.");

        bool overlapExists = await context.ItemAssignmentHistories
            .AnyAsync(a =>
                a.ItemId == model.ItemId &&
                (
                    (model.AssignedUntil == null || a.AssignedFrom <= model.AssignedUntil) &&
                    (a.AssignedUntil == null || a.AssignedUntil >= model.AssignedFrom)
                ));

        if (overlapExists)
            throw new ConflictException("An overlapping assignment already exists for this item.");

        var assignment = mapper.MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(model);

        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        assignment = await context.ItemAssignmentHistories
            .AsNoTracking()
            .SingleAsync(a => a.Id == assignment.Id);

        return mapper.MapItemAssignmentHistoryToItemAssignmentHistoryModel(assignment);
    }

    public async Task<List<ItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId)
    {
        var itemExists = await context.Items.AnyAsync(i => i.Id == itemId);
        if (!itemExists)
            throw new NotFoundException($"Item with ID {itemId} not found.");

        var entities = await context.ItemAssignmentHistories
            .Where(a => a.ItemId == itemId)
            .OrderByDescending(a => a.AssignedFrom)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapItemAssignmentHistorysToItemAssignmentHistoryModels(entities);
    }

    public async Task<List<ItemAssignmentHistoryModel>> GetAllAssignmentsAsync()
    {
        var entities = await context.ItemAssignmentHistories
            .AsNoTracking()
            .OrderByDescending(a => a.AssignedFrom)
            .ToListAsync();

        return mapper.MapItemAssignmentHistorysToItemAssignmentHistoryModels(entities);
    }

    public async Task<ItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id)
    {
        var entity = await context.ItemAssignmentHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return entity is null ? null : mapper.MapItemAssignmentHistoryToItemAssignmentHistoryModel(entity);
    }

    public async Task<bool> UpdateAssignmentAsync(Guid id, CreateOrUpdateItemAssignmentHistoryModel model)
    {
        var entity = await context.ItemAssignmentHistories
            .FindAsync(id);
        if (entity is null)
            return false;

        if (!await context.Items.AnyAsync(i => i.Id == model.ItemId))
            throw new BadRequestException($"Item with ID '{model.ItemId}' does not exist.");

        if (!await context.Persons.AnyAsync(p => p.Id == model.PersonId))
            throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        _ = await userService.GetUserByIdAsync(model.AssignedById) ?? throw new BadRequestException($"User with ID '{model.AssignedById}' does not exist.");

        bool overlapExists = await context.ItemAssignmentHistories
            .AnyAsync(a =>
                a.Id != id &&
                a.ItemId == model.ItemId &&
                (
                    (model.AssignedUntil == null || a.AssignedFrom <= model.AssignedUntil) &&
                    (a.AssignedUntil == null || a.AssignedUntil >= model.AssignedFrom)
                ));

        if (overlapExists)
            throw new ConflictException("An overlapping assignment already exists for this item.");

        mapper.MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        var entity = await context.ItemAssignmentHistories.FindAsync(id);
        if (entity is null)
            return false;

        context.ItemAssignmentHistories.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}