using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database;

public class AppDbContext : DbContext
{
    private readonly TenantProvider? _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, TenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<Variant> Variants => Set<Variant>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    public DbSet<Maintenance> Maintenances => Set<Maintenance>();

    public DbSet<ItemAssignmentHistory> ItemAssignmentHistories => Set<ItemAssignmentHistory>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<User> Users => Set<User>();

    public DbSet<ProductType> ProductTypes => Set<ProductType>();

    public DbSet<MaintenanceType> MaintenanceTypes => Set<MaintenanceType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filter for all IHasTenant entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                // Create the filter expression: entity => entity.TenantId == currentTenantId
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IHasTenant.TenantId));
                var tenantIdValue = System.Linq.Expressions.Expression.Constant(_tenantProvider?.TenantId);
                var equals = System.Linq.Expressions.Expression.Equal(property, tenantIdValue);
                var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically assign TenantId to new entities that implement IHasTenant
        if (_tenantProvider?.HasTenant ?? false)
        {
            var entries = ChangeTracker.Entries<IHasTenant>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Entity.TenantId))
                {
                    entry.Entity.TenantId = _tenantProvider.TenantId!;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
