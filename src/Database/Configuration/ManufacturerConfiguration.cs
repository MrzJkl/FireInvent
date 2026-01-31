using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Manufacturer entity.
/// Defines restrict delete behavior: Manufacturer is protected master data that cannot be deleted if Products reference it.
/// </summary>
public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        // Restrict: Deleting a Manufacturer is prevented if Products reference it (protecting master data)
        builder
            .HasMany(m => m.Products)
            .WithOne(p => p.Manufacturer)
            .HasForeignKey(p => p.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(m => m.CreatedBy)
            .WithMany()
            .HasForeignKey(m => m.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(m => m.ModifiedBy)
            .WithMany()
            .HasForeignKey(m => m.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
