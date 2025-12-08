using FireInvent.Contract;
using FireInvent.Database;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared;

internal static class TestHelper
{
    public static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    internal static AppDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        var testTenantProvider = new TenantProvider
        {
            TenantId = TestTenantId
        };
        
        return new AppDbContext(options, testTenantProvider);
    }
}
