using FlameGuardLaundry.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Database;

public class GearDbContext(DbContextOptions<GearDbContext> options) : DbContext(options)
{
    public DbSet<ClothingItem> ClothingItems => Set<ClothingItem>();

    public DbSet<ClothingVariant> ClothingVariants => Set<ClothingVariant>();

    public DbSet<ClothingModel> ClothingModels => Set<ClothingModel>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
}
