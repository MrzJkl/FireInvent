﻿using FireInvent.Contract;
using FireInvent.Database.Configuration;
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

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Visit> Visits => Set<Visit>();

    public DbSet<VisitItem> VisitItems => Set<VisitItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations with cascade delete rules
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new VisitConfiguration());
        modelBuilder.ApplyConfiguration(new VisitItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new VariantConfiguration());
        modelBuilder.ApplyConfiguration(new ItemConfiguration());
        modelBuilder.ApplyConfiguration(new ItemAssignmentHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenanceConfiguration());
        modelBuilder.ApplyConfiguration(new ProductTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenanceTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ManufacturerConfiguration());
        modelBuilder.ApplyConfiguration(new StorageLocationConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());

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
        var hasTenant = _tenantProvider is not null && _tenantProvider.TenantId.HasValue && _tenantProvider.TenantId.Value != Guid.Empty;
        var hasUser = _tenantProvider is not null && _tenantProvider.UserId.HasValue && _tenantProvider.UserId.Value != Guid.Empty;

        if (hasTenant)
        {
            foreach (var entry in ChangeTracker.Entries<IHasTenant>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty))
            {
                entry.Entity.TenantId = _tenantProvider!.TenantId!.Value;
            }
        }

        if (hasUser)
        {
            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(x => x.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                    entry.Property(x => x.CreatedById).CurrentValue = _tenantProvider!.UserId!.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.ModifiedAt).CurrentValue = DateTimeOffset.UtcNow;
                    entry.Property(x => x.ModifiedById).CurrentValue = _tenantProvider!.UserId!.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
