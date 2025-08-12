using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }

    public DbSet<ClothingItem> ClothingItems => Set<ClothingItem>();

    public DbSet<ClothingVariant> ClothingVariants => Set<ClothingVariant>();

    public DbSet<ClothingProduct> ClothingProducts => Set<ClothingProduct>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    public DbSet<Maintenance> Maintenances => Set<Maintenance>();

    public DbSet<ClothingItemAssignmentHistory> ClothingItemAssignmentHistories => Set<ClothingItemAssignmentHistory>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<User> Users => Set<User>();
}
