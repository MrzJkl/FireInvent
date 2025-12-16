using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class TenantMapper : BaseMapper
{
    public partial TenantModel MapTenantToTenantModel(Tenant tenant);

    [MapValue(nameof(Tenant.Id), Use = nameof(NewGuid))]
    [MapValue(nameof(Tenant.CreatedAt), Use = nameof(UtcNow))]
    public partial Tenant MapCreateOrUpdateTenantModelToTenant(CreateOrUpdateTenantModel createTenantModel);

    public partial List<TenantModel> MapTenantsToTenantModels(List<Tenant> tenants);

    public partial void MapCreateOrUpdateTenantModelToTenant(CreateOrUpdateTenantModel source, Tenant target);
}
