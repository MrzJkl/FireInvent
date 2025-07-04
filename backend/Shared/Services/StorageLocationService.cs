using AutoMapper;
using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class StorageLocationService(GearDbContext context, IMapper mapper)
{
    public async Task<StorageLocationModel> CreateStorageLocationAsync(CreateStorageLocationModel model)
    {
        var exists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name);

        if (exists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        var location = mapper.Map<StorageLocation>(model);
        location.Id = Guid.NewGuid();

        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        return mapper.Map<StorageLocationModel>(location);
    }

    public async Task<List<StorageLocationModel>> GetAllStorageLocationsAsync()
    {
        var locations = await context.StorageLocations
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<StorageLocationModel>>(locations);
    }

    public async Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id)
    {
        var location = await context.StorageLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return location is null ? null : mapper.Map<StorageLocationModel>(location);
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

        mapper.Map(model, location);

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