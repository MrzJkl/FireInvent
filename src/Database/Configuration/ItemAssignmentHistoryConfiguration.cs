using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the ItemAssignmentHistory entity.
/// Defines cascade delete behavior: when an Item is deleted, all its assignment histories are deleted.
/// </summary>
public class ItemAssignmentHistoryConfiguration : IEntityTypeConfiguration<ItemAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<ItemAssignmentHistory> builder)
    {
        // Cascade: Deleting an Item deletes all its assignment histories
        builder
            .HasOne(ia => ia.Item)
            .WithMany(i => i.Assignments)
            .HasForeignKey(ia => ia.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ia => ia.Person)
            .WithMany(p => p.AssignedItems)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(ia => ia.StorageLocation)
            .WithMany(sl => sl.AssignedItems)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(ia => ia.CreatedBy)
            .WithMany()
            .HasForeignKey(ia => ia.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(ia => ia.ModifiedBy)
            .WithMany()
            .HasForeignKey(ia => ia.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
