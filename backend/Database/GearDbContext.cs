using FireInvent.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database;

public class GearDbContext(DbContextOptions<GearDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<ClothingItem> ClothingItems => Set<ClothingItem>();

    public DbSet<ClothingVariant> ClothingVariants => Set<ClothingVariant>();

    public DbSet<ClothingProduct> ClothingProducts => Set<ClothingProduct>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    public DbSet<Maintenance> Maintenances => Set<Maintenance>();

    public DbSet<ClothingItemAssignmentHistory> ClothingItemAssignmentHistories => Set<ClothingItemAssignmentHistory>();
}
