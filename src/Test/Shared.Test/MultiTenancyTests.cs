using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared;

public class MultiTenancyTests
{
    [Fact]
    public async Task EntitiesAreIsolatedByTenant()
    {
        // Arrange - Create two separate contexts with different tenants
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        var tenant1Provider = new TenantProvider { TenantId = tenant1Id };
        var tenant2Provider = new TenantProvider { TenantId = tenant2Id };

        using var context1 = new AppDbContext(options, tenant1Provider);
        using var context2 = new AppDbContext(options, tenant2Provider);

        // Act - Create entities in different tenant contexts
        var person1 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Tenant1",
        };
        context1.Persons.Add(person1);
        await context1.SaveChangesAsync();

        var person2 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Tenant2",
        };
        context2.Persons.Add(person2);
        await context2.SaveChangesAsync();

        // Assert - Each tenant can only see their own data
        var tenant1Persons = await context1.Persons.ToListAsync();
        var tenant2Persons = await context2.Persons.ToListAsync();

        Assert.Single(tenant1Persons);
        Assert.Equal("Alice", tenant1Persons[0].FirstName);
        Assert.Equal(tenant1Id, tenant1Persons[0].TenantId);

        Assert.Single(tenant2Persons);
        Assert.Equal("Bob", tenant2Persons[0].FirstName);
        Assert.Equal(tenant2Id, tenant2Persons[0].TenantId);
    }

    [Fact]
    public async Task TenantIdIsAutomaticallyAssignedOnSave()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenantId = Guid.NewGuid();

        var tenantProvider = new TenantProvider { TenantId = tenantId };
        using var context = new AppDbContext(options, tenantProvider);

        // Act - Create entity without explicitly setting TenantId
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Test Department",
            TenantId = Guid.Empty // Empty, should be auto-assigned
        };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Assert - TenantId was automatically assigned
        var saved = await context.Departments.FirstAsync();
        Assert.Equal(tenantId, saved.TenantId);
    }

    [Fact]
    public async Task IgnoreQueryFiltersAllowsCrossTenantQueries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        var tenant1Provider = new TenantProvider { TenantId = tenant1Id };
        var tenant2Provider = new TenantProvider { TenantId = tenant2Id };

        // Create entities in different tenants
        using (var context1 = new AppDbContext(options, tenant1Provider))
        {
            context1.StorageLocations.Add(new StorageLocation
            {
                Id = Guid.NewGuid(),
                Name = "Location 1"
            });
            await context1.SaveChangesAsync();
        }

        using (var context2 = new AppDbContext(options, tenant2Provider))
        {
            context2.StorageLocations.Add(new StorageLocation
            {
                Id = Guid.NewGuid(),
                Name = "Location 2"
            });
            await context2.SaveChangesAsync();
        }

        // Act - Query with IgnoreQueryFilters from tenant-1 context
        using var queryContext = new AppDbContext(options, tenant1Provider);
        var filteredResults = await queryContext.StorageLocations.ToListAsync();
        var allResults = await queryContext.StorageLocations.IgnoreQueryFilters().ToListAsync();

        // Assert
        Assert.Single(filteredResults); // Only sees tenant-1 data
        Assert.Equal(2, allResults.Count); // Sees all tenants' data
    }
}
