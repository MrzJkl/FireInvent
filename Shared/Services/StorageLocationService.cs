using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class StorageLocationService(GearDbContext context)
{
    public async Task<StorageLocationModel> CreateStorageLocationAsync(StorageLocationModel model)
    {
        var exists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name);

        if (exists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        var location = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Remarks = model.Remarks
        };

        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        return new StorageLocationModel
        {
            Id = location.Id,
            Name = location.Name,
            Remarks = location.Remarks
        };
    }

    public async Task<List<StorageLocationModel>> GetAllStorageLocationsAsync()
    {
        return await context.StorageLocations
            .AsNoTracking()
            .Select(s => new StorageLocationModel
            {
                Id = s.Id,
                Name = s.Name,
                Remarks = s.Remarks
            })
            .ToListAsync();
    }

    public async Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id)
    {
        var location = await context.StorageLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (location is null)
            return null;

        return new StorageLocationModel
        {
            Id = location.Id,
            Name = location.Name,
            Remarks = location.Remarks
        };
    }

    public async Task<bool> UpdateStorageLocationAsync(StorageLocationModel model)
    {
        var location = await context.StorageLocations.FindAsync(model.Id);
        if (location is null)
            return false;

        var nameExists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name && s.Id != model.Id);

        if (nameExists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        location.Name = model.Name;
        location.Remarks = model.Remarks;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteStorageLocationAsync(Guid id)
    {
        var location = await context.StorageLocations.FindAsync(id);
        if (location is null)
            return false;

        context.StorageLocations.Remove(location);
        await context.SaveChangesAsync();
        return true;
    }
}