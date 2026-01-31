using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the StorageLocation entity.
/// Defines restrict delete behavior: StorageLocation is protected and cannot be deleted if Items are assigned to it.
/// </summary>
public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> builder)
    {
        // Restrict: Deleting a StorageLocation is prevented if Items are stored there
        builder
            .HasMany(sl => sl.AssignedItems)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(sl => sl.CreatedBy)
            .WithMany()
            .HasForeignKey(sl => sl.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(sl => sl.ModifiedBy)
            .WithMany()
            .HasForeignKey(sl => sl.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
