using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Item entity.
/// Defines restrict delete behavior: Items cannot be orphaned when critical entities are deleted.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        // Cascade: Deleting an Item deletes all its assignment histories
        builder
            .HasMany(i => i.Assignments)
            .WithOne(ia => ia.Item)
            .HasForeignKey(ia => ia.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cascade: Deleting an Item deletes all its maintenance records
        builder
            .HasMany(i => i.Maintenances)
            .WithOne(m => m.Item)
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: Deleting a Variant is prevented if Items exist
        builder
            .HasOne(i => i.Variant)
            .WithMany(v => v.Items)
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(i => i.ModifiedBy)
            .WithMany()
            .HasForeignKey(i => i.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
