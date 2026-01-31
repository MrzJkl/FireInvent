using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the MaintenanceType entity.
/// Defines restrict delete behavior: MaintenanceType is protected master data that cannot be deleted if Maintenances reference it.
/// </summary>
public class MaintenanceTypeConfiguration : IEntityTypeConfiguration<MaintenanceType>
{
    public void Configure(EntityTypeBuilder<MaintenanceType> builder)
    {
        // Restrict: Deleting a MaintenanceType is prevented if Maintenances reference it (protecting master data)
        builder
            .HasMany<Maintenance>()
            .WithOne(m => m.Type)
            .HasForeignKey(m => m.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(mt => mt.CreatedBy)
            .WithMany()
            .HasForeignKey(mt => mt.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(mt => mt.ModifiedBy)
            .WithMany()
            .HasForeignKey(mt => mt.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
