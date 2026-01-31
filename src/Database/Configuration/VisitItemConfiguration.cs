using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the VisitItem entity.
/// Defines restrict delete behavior: VisitItems cannot be orphaned when Products are deleted.
/// </summary>
public class VisitItemConfiguration : IEntityTypeConfiguration<VisitItem>
{
    public void Configure(EntityTypeBuilder<VisitItem> builder)
    {
        // Cascade: Deleting a Visit deletes all its VisitItems
        builder
            .HasOne(vi => vi.Visit)
            .WithMany(v => v.Items)
            .HasForeignKey(vi => vi.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: Deleting a Product is prevented if VisitItems reference it
        builder
            .HasOne(vi => vi.Product)
            .WithMany()
            .HasForeignKey(vi => vi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(vi => vi.CreatedBy)
            .WithMany()
            .HasForeignKey(vi => vi.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(vi => vi.ModifiedBy)
            .WithMany()
            .HasForeignKey(vi => vi.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
