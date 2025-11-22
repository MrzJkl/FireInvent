using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class StorageLocationService(AppDbContext context, StorageLocationMapper mapper) : IStorageLocationService
{
    public async Task<StorageLocationModel> CreateStorageLocationAsync(CreateOrUpdateStorageLocationModel model)
    {
        var exists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name);

        if (exists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        var location = mapper.MapCreateOrUpdateStorageLocationModelToStorageLocation(model);
        location.Id = Guid.NewGuid();

        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        return mapper.MapStorageLocationToStorageLocationModel(location);
    }

    public async Task<List<StorageLocationModel>> GetAllStorageLocationsAsync()
    {
        var locations = await context.StorageLocations
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapStorageLocationsToStorageLocationModels(locations);
    }

    public async Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id)
    {
        var location = await context.StorageLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return location is null ? null : mapper.MapStorageLocationToStorageLocationModel(location);
    }

    public async Task<bool> UpdateStorageLocationAsync(Guid id, CreateOrUpdateStorageLocationModel model)
    {
        var location = await context.StorageLocations.FindAsync(id);
        if (location is null)
            return false;

        var nameExists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name && s.Id != id);

        if (nameExists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        mapper.MapCreateOrUpdateStorageLocationModelToStorageLocation(model, location);

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