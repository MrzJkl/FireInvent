using FireInvent.Contract.Extensions;
using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FireInvent.Shared.Services;

public class TenantService(
    AppDbContext context,
    TenantMapper mapper,
    ILogger<TenantService> logger,
    IKeycloakTenantService keycloakTenantService) : ITenantService
{
    public async Task<TenantModel> CreateTenantAsync(CreateOrUpdateTenantModel model)
    {
        // Use IgnoreQueryFilters to check across all tenants
        var nameExists = await context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("A tenant with the same name already exists.");

        logger.LogInformation("Creating Keycloak organization for tenant: {TenantName}", model.Name.SanitizeForLogging());
            
        var organizationIdFromKeycloak = await keycloakTenantService.CreateTenantOrganizationAsync(
            model.Name,
            model.Description
        );

        var tenant = mapper.MapCreateOrUpdateTenantModelToTenant(model);
        tenant.Id = organizationIdFromKeycloak;

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

        var nameExists = await context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id != id && t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("Another tenant with the same name already exists.");

        await keycloakTenantService.UpdateTenantOrganizationNameAsync(tenant.Id, model.Name, model.Description);

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

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();
        return true;
    }
}
