using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> ClothingItems => Set<Item>();

    public DbSet<Variant> ClothingVariants => Set<Variant>();

    public DbSet<Product> ClothingProducts => Set<Product>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    public DbSet<Maintenance> Maintenances => Set<Maintenance>();

    public DbSet<ItemAssignmentHistory> ClothingItemAssignmentHistories => Set<ItemAssignmentHistory>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<User> Users => Set<User>();
}
