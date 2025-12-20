using FireInvent.Contract;
using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database;

public class AppDbContext : DbContext
{
    private readonly UserContextProvider? _tenantProvider;

    /// <summary>
    /// Parameterless constructor for EF Core migrations and tooling.
    /// Should not be used in application code.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Constructor with UserContextProvider for runtime usage.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options, UserContextProvider userContextProvider) : base(options)
    {
        _tenantProvider = userContextProvider;
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

    public DbSet<ProductType> ProductTypes => Set<ProductType>();

    public DbSet<MaintenanceType> MaintenanceTypes => Set<MaintenanceType>();

    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filter for all IHasTenant entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                // entity => _tenantProvider == null || !_tenantProvider.TenantId.HasValue || entity.TenantId == _tenantProvider.TenantId.Value
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IHasTenant.TenantId));

                var thisConstant = System.Linq.Expressions.Expression.Constant(this);
                var tenantProviderField = System.Linq.Expressions.Expression.Field(thisConstant, "_tenantProvider");

                var tenantProviderIsNull = System.Linq.Expressions.Expression.Equal(
                    tenantProviderField,
                    System.Linq.Expressions.Expression.Constant(null, typeof(UserContextProvider)));

                var tenantIdProperty = System.Linq.Expressions.Expression.Property(tenantProviderField, nameof(UserContextProvider.TenantId));
                var tenantIdHasValue = System.Linq.Expressions.Expression.Property(tenantIdProperty, nameof(Nullable<Guid>.HasValue));
                var tenantIdValue = System.Linq.Expressions.Expression.Property(tenantIdProperty, nameof(Nullable<Guid>.Value));

                var tenantIdIsNotSet = System.Linq.Expressions.Expression.Not(tenantIdHasValue);

                var tenantIdEquals = System.Linq.Expressions.Expression.Equal(property, tenantIdValue);

                var filterExpression = System.Linq.Expressions.Expression.OrElse(
                    System.Linq.Expressions.Expression.OrElse(tenantProviderIsNull, tenantIdIsNotSet),
                    tenantIdEquals);

                var lambda = System.Linq.Expressions.Expression.Lambda(filterExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically assign TenantId to new entities that implement IHasTenant
        if (_tenantProvider is not null && _tenantProvider.TenantId.HasValue && _tenantProvider.TenantId.Value != Guid.Empty)
        {
            var entries = ChangeTracker.Entries<IHasTenant>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _tenantProvider.TenantId.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
