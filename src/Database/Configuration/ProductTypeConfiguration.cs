using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the ProductType entity.
/// Defines restrict delete behavior: ProductType is protected master data that cannot be deleted if Products reference it.
/// </summary>
public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        // Restrict: Deleting a ProductType is prevented if Products reference it (protecting master data)
        builder
            .HasMany<Product>()
            .WithOne(p => p.Type)
            .HasForeignKey(p => p.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(pt => pt.CreatedBy)
            .WithMany()
            .HasForeignKey(pt => pt.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(pt => pt.ModifiedBy)
            .WithMany()
            .HasForeignKey(pt => pt.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
