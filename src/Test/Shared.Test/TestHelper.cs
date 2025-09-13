using FireInvent.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

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

    internal static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), new NullLoggerFactory());
        return config.CreateMapper();
    }
}
