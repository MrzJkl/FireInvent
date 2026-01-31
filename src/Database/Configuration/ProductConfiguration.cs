using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Product entity.
/// Defines cascade delete behavior: when a Product is deleted, all related Variants are deleted.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Cascade: Deleting a Product deletes all its Variants
        builder
            .HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: Deleting a ProductType is prevented if Products reference it
        builder
            .HasOne(p => p.Type)
            .WithMany()
            .HasForeignKey(p => p.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: Deleting a Manufacturer is prevented if Products reference it
        builder
            .HasOne(p => p.Manufacturer)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(p => p.ModifiedBy)
            .WithMany()
            .HasForeignKey(p => p.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
