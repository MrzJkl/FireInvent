using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FireInvent.Shared.Services;

public class TenantService : ITenantService
{
    private readonly AppDbContext _context;
    private readonly TenantMapper _mapper;
    private readonly IKeycloakTenantService _keycloakTenantService;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        AppDbContext context, 
        TenantMapper mapper,
        ILogger<TenantService> logger,
        IKeycloakTenantService keycloakTenantService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _keycloakTenantService = keycloakTenantService;
    }

    public async Task<TenantModel> CreateTenantAsync(CreateOrUpdateTenantModel model)
    {
        // Use IgnoreQueryFilters to check across all tenants
        var nameExists = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("A tenant with the same name already exists.");

        _logger.LogInformation("Creating Keycloak organization for tenant: {TenantName}", model.Name);
            
        var organizationIdFromKeycloak = await _keycloakTenantService.CreateTenantOrganizationAsync(
            model.Name,
            model.Description
        );

        var tenant = _mapper.MapCreateOrUpdateTenantModelToTenant(model);
        tenant.Id = organizationIdFromKeycloak;

        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        return _mapper.MapTenantToTenantModel(tenant);
    }

    public async Task<List<TenantModel>> GetAllTenantsAsync()
    {
        // Use IgnoreQueryFilters to get all tenants across the system
        var tenants = await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        return _mapper.MapTenantsToTenantModels(tenants);
    }

    public async Task<TenantModel?> GetTenantByIdAsync(Guid id)
    {
        // Use IgnoreQueryFilters to get any tenant by ID
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return tenant is null ? null : _mapper.MapTenantToTenantModel(tenant);
    }

    public async Task<bool> UpdateTenantAsync(Guid id, CreateOrUpdateTenantModel model)
    {
        // Use IgnoreQueryFilters to find any tenant
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant is null)
            return false;

        var nameExists = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id != id && t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("Another tenant with the same name already exists.");

        await _keycloakTenantService.UpdateTenantOrganizationNameAsync(tenant.Id, model.Name, model.Description);

        _mapper.MapCreateOrUpdateTenantModelToTenant(model, tenant);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTenantAsync(Guid id)
    {
        // Use IgnoreQueryFilters to find any tenant
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant is null)
            return false;

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
        return true;
    }
}
