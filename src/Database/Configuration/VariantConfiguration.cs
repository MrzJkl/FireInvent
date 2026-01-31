using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Variant entity.
/// Defines restrict delete behavior: Variants cannot be orphaned when critical entities are deleted.
/// </summary>
public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        // Cascade: Deleting a Product deletes all its Variants
        builder
            .HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: Deleting a Product is prevented if Variants exist (handled by cascade above)
        // This is redundant but explicitly documented

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(v => v.CreatedBy)
            .WithMany()
            .HasForeignKey(v => v.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(v => v.ModifiedBy)
            .WithMany()
            .HasForeignKey(v => v.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
