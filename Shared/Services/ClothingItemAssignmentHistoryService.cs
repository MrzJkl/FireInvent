using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services
{
    public class ClothingItemAssignmentHistoryService(GearDbContext context)
    {
        public async Task<ClothingItemAssignmentHistoryModel> CreateAssignmentAsync(ClothingItemAssignmentHistoryModel model)
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

            var entity = new ClothingItemAssignmentHistory
            {
                Id = Guid.NewGuid(),
                ItemId = model.ItemId,
                PersonId = model.PersonId,
                AssignedFrom = model.AssignedFrom,
                AssignedUntil = model.AssignedUntil
            };

            context.ClothingItemAssignmentHistories.Add(entity);
            await context.SaveChangesAsync();

            return MapToModel(entity);
        }

        public async Task<List<ClothingItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId)
        {
            var itemExists = await context.ClothingItems.AnyAsync(i => i.Id == itemId);
            if (!itemExists)
                throw new NotFoundException($"ClothingItem with ID {itemId} not found.");

            return await context.ClothingItemAssignmentHistories
                .Where(a => a.ItemId == itemId)
                .OrderByDescending(a => a.AssignedFrom)
                .AsNoTracking()
                .Select(a => new ClothingItemAssignmentHistoryModel
                {
                    Id = a.Id,
                    ItemId = a.ItemId,
                    PersonId = a.PersonId,
                    AssignedFrom = a.AssignedFrom,
                    AssignedUntil = a.AssignedUntil
                })
                .ToListAsync();
        }

        public async Task<List<ClothingItemAssignmentHistoryModel>> GetAllAssignmentsAsync()
        {
            return await context.ClothingItemAssignmentHistories
                .AsNoTracking()
                .Select(a => new ClothingItemAssignmentHistoryModel
                {
                    Id = a.Id,
                    ItemId = a.ItemId,
                    PersonId = a.PersonId,
                    AssignedFrom = a.AssignedFrom,
                    AssignedUntil = a.AssignedUntil
                })
                .ToListAsync();
        }

        public async Task<ClothingItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id)
        {
            var entity = await context.ClothingItemAssignmentHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return entity is null ? null : MapToModel(entity);
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

            entity.ItemId = model.ItemId;
            entity.PersonId = model.PersonId;
            entity.AssignedFrom = model.AssignedFrom;
            entity.AssignedUntil = model.AssignedUntil;

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

        private static ClothingItemAssignmentHistoryModel MapToModel(ClothingItemAssignmentHistory entity) => new()
        {
            Id = entity.Id,
            ItemId = entity.ItemId,
            PersonId = entity.PersonId,
            AssignedFrom = entity.AssignedFrom,
            AssignedUntil = entity.AssignedUntil
        };
    }
}
