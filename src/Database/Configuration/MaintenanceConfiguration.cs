using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Maintenance entity.
/// Defines restrict delete behavior: MaintenanceType and Items are protected from deletion.
/// </summary>
public class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> builder)
    {
        // Restrict: Deleting a MaintenanceType is prevented if Maintenances reference it
        builder
            .HasOne(m => m.Type)
            .WithMany()
            .HasForeignKey(m => m.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: Deleting an Item is prevented if Maintenances exist for it
        builder
            .HasOne(m => m.Item)
            .WithMany(i => i.Maintenances)
            .HasForeignKey(m => m.ItemId)
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
