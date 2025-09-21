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
        return new AppDbContext(options);
    }
}
