using AutoMapper;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ClothingItemAssignmentHistoryService(AppDbContext context, IMapper mapper)
{
    public async Task<ClothingItemAssignmentHistoryModel> CreateAssignmentAsync(CreateClothingItemAssignmentHistoryModel model)
    {
        if (!await context.ClothingItems.AnyAsync(i => i.Id == model.ItemId))
            throw new BadRequestException($"ClothingItem with ID '{model.ItemId}' does not exist.");

        if (!await context.Persons.AnyAsync(p => p.Id == model.PersonId))
            throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        bool overlapExists = await context.ClothingItemAssignmentHistories
            .AnyAsync(a =>
                a.ItemId == model.ItemId &&
                (
                    (model.AssignedUntil == null || a.AssignedFrom <= model.AssignedUntil) &&
                    (a.AssignedUntil == null || a.AssignedUntil >= model.AssignedFrom)
                ));

        if (overlapExists)
            throw new ConflictException("An overlapping assignment already exists for this item.");

        var entity = mapper.Map<ClothingItemAssignmentHistory>(model);
        entity.Id = Guid.NewGuid();

        context.ClothingItemAssignmentHistories.Add(entity);
        await context.SaveChangesAsync();

        return mapper.Map<ClothingItemAssignmentHistoryModel>(entity);
    }

    public async Task<List<ClothingItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId)
    {
        var itemExists = await context.ClothingItems.AnyAsync(i => i.Id == itemId);
        if (!itemExists)
            throw new NotFoundException($"ClothingItem with ID {itemId} not found.");

        var entities = await context.ClothingItemAssignmentHistories
            .Where(a => a.ItemId == itemId)
            .OrderByDescending(a => a.AssignedFrom)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<ClothingItemAssignmentHistoryModel>>(entities);
    }

    public async Task<List<ClothingItemAssignmentHistoryModel>> GetAllAssignmentsAsync()
    {
        var entities = await context.ClothingItemAssignmentHistories
            .AsNoTracking()
            .OrderByDescending(a => a.AssignedFrom)
            .ToListAsync();

        return mapper.Map<List<ClothingItemAssignmentHistoryModel>>(entities);
    }

    public async Task<ClothingItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id)
    {
        var entity = await context.ClothingItemAssignmentHistories
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return entity is null ? null : mapper.Map<ClothingItemAssignmentHistoryModel>(entity);
    }

    public async Task<bool> UpdateAssignmentAsync(ClothingItemAssignmentHistoryModel model)
    {
        var entity = await context.ClothingItemAssignmentHistories.FindAsync(model.Id);
        if (entity is null)
            return false;

        if (!await context.ClothingItems.AnyAsync(i => i.Id == model.ItemId))
            throw new BadRequestException($"ClothingItem with ID '{model.ItemId}' does not exist.");

        if (!await context.Persons.AnyAsync(p => p.Id == model.PersonId))
            throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        bool overlapExists = await context.ClothingItemAssignmentHistories
            .AnyAsync(a =>
                a.Id != model.Id &&
                a.ItemId == model.ItemId &&
                (
                    (model.AssignedUntil == null || a.AssignedFrom <= model.AssignedUntil) &&
                    (a.AssignedUntil == null || a.AssignedUntil >= model.AssignedFrom)
                ));

        if (overlapExists)
            throw new ConflictException("An overlapping assignment already exists for this item.");

        mapper.Map(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        var entity = await context.ClothingItemAssignmentHistories.FindAsync(id);
        if (entity is null)
            return false;

        context.ClothingItemAssignmentHistories.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}