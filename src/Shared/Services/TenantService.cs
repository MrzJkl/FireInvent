using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing tenants. This service bypasses tenant filtering
/// as it operates at the system/master level.
/// Optionally integrates with Keycloak to create/manage realms.
/// </summary>
public class TenantService : ITenantService
{
    private readonly AppDbContext _context;
    private readonly TenantMapper _mapper;
    private readonly IKeycloakTenantService? _keycloakTenantService;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        AppDbContext context, 
        TenantMapper mapper,
        ILogger<TenantService> logger,
        IKeycloakTenantService? keycloakTenantService = null)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _keycloakTenantService = keycloakTenantService;
    }

    public async Task<TenantModel> CreateTenantAsync(CreateOrUpdateTenantModel model)
    {
        // Use IgnoreQueryFilters to check across all tenants
        var realmExists = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Realm == model.Realm);

        if (realmExists)
            throw new ConflictException("A tenant with the same realm already exists.");

        var nameExists = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("A tenant with the same name already exists.");

        // Optionally create Keycloak realm if service is available
        if (_keycloakTenantService != null)
        {
            _logger.LogInformation("Creating Keycloak realm for tenant: {Realm}", model.Realm);
            
            var realmCreated = await _keycloakTenantService.CreateTenantRealmAsync(model.Realm, model.Name);
            
            if (!realmCreated)
            {
                throw new InvalidOperationException($"Failed to create Keycloak realm: {model.Realm}");
            }

            // Configure the newly created realm with default settings
            try
            {
                await _keycloakTenantService.ConfigureTenantRealmAsync(model.Realm);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to configure Keycloak realm {Realm}, but continuing with tenant creation", model.Realm);
            }
        }
        else
        {
            _logger.LogWarning("KeycloakTenantService not available. Realm must be created manually in Keycloak.");
        }

        var tenant = _mapper.MapCreateOrUpdateTenantModelToTenant(model);

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

        var realmExists = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id != id && t.Realm == model.Realm);

        if (realmExists)
            throw new ConflictException("Another tenant with the same realm already exists.");

        var nameExists = await _context.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id != id && t.Name == model.Name);

        if (nameExists)
            throw new ConflictException("Another tenant with the same name already exists.");

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

        // Check if there are any entities associated with this tenant
        // This prevents orphaned data
        var hasItems = await _context.Items
            .IgnoreQueryFilters()
            .AnyAsync(i => i.TenantId == id);

        if (hasItems)
            throw new ConflictException("Cannot delete tenant with existing data. Please delete all associated data first.");

        var hasUsers = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.TenantId == id);

        if (hasUsers)
            throw new ConflictException("Cannot delete tenant with existing users. Please delete all associated users first.");

        // Optionally delete Keycloak realm if service is available
        if (_keycloakTenantService != null)
        {
            _logger.LogInformation("Deleting Keycloak realm for tenant: {Realm}", tenant.Realm);
            
            var realmDeleted = await _keycloakTenantService.DeleteTenantRealmAsync(tenant.Realm);
            
            if (!realmDeleted)
            {
                _logger.LogWarning("Failed to delete Keycloak realm {Realm}, but continuing with tenant deletion", tenant.Realm);
            }
        }

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
        return true;
    }
}
