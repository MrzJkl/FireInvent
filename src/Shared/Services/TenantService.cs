using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing tenants. This service bypasses tenant filtering
/// as it operates at the system/master level.
/// </summary>
public class TenantService(AppDbContext context, TenantMapper mapper) : ITenantService
{
    public async Task<TenantModel> CreateTenantAsync(CreateOrUpdateTenantModel model)
    {
        // Use IgnoreQueryFilters to check across all tenants
        var realmExists = await context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Realm == model.Realm);

        if (realmExists)
            throw new ConflictException("A tenant with the same realm already exists.");

        var nameExists = await context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("A tenant with the same name already exists.");

        var tenant = mapper.MapCreateOrUpdateTenantModelToTenant(model);

        await context.Tenants.AddAsync(tenant);
        await context.SaveChangesAsync();

        return mapper.MapTenantToTenantModel(tenant);
    }

    public async Task<List<TenantModel>> GetAllTenantsAsync()
    {
        // Use IgnoreQueryFilters to get all tenants across the system
        var tenants = await context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        return mapper.MapTenantsToTenantModels(tenants);
    }

    public async Task<TenantModel?> GetTenantByIdAsync(Guid id)
    {
        // Use IgnoreQueryFilters to get any tenant by ID
        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return tenant is null ? null : mapper.MapTenantToTenantModel(tenant);
    }

    public async Task<bool> UpdateTenantAsync(Guid id, CreateOrUpdateTenantModel model)
    {
        // Use IgnoreQueryFilters to find any tenant
        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant is null)
            return false;

        var realmExists = await context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id != id && t.Realm == model.Realm);

        if (realmExists)
            throw new ConflictException("Another tenant with the same realm already exists.");

        var nameExists = await context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id != id && t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("Another tenant with the same name already exists.");

        mapper.MapCreateOrUpdateTenantModelToTenant(model, tenant);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTenantAsync(Guid id)
    {
        // Use IgnoreQueryFilters to find any tenant
        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant is null)
            return false;

        // Check if there are any entities associated with this tenant
        // This prevents orphaned data
        var hasItems = await context.Items
            .IgnoreQueryFilters()
            .AnyAsync(i => i.TenantId == id);

        if (hasItems)
            throw new ConflictException("Cannot delete tenant with existing data. Please delete all associated data first.");

        var hasUsers = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.TenantId == id);

        if (hasUsers)
            throw new ConflictException("Cannot delete tenant with existing users. Please delete all associated users first.");

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();
        return true;
    }
}
