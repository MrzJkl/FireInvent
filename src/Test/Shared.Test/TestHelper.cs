using FireInvent.Database;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared;

internal static class TestHelper
{
    internal static AppDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        // Create a test tenant provider with a default test tenant ID
        var tenantProvider = new TenantProvider
        {
            TenantId = "test-tenant"
        };
        
        return new AppDbContext(options, tenantProvider);
    }
}
