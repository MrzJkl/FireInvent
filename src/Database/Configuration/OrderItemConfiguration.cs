using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the OrderItem entity.
/// Defines restrict delete behavior: OrderItems cannot be orphaned when critical entities are deleted.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Cascade: Deleting an Order deletes all its OrderItems
        builder
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: Deleting a Variant is prevented if OrderItems reference it
        builder
            .HasOne(oi => oi.Variant)
            .WithMany()
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: Deleting a Person sets the reference to null (optional assignment)
        builder
            .HasOne(oi => oi.Person)
            .WithMany()
            .HasForeignKey(oi => oi.PersonId)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(oi => oi.CreatedBy)
            .WithMany()
            .HasForeignKey(oi => oi.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(oi => oi.ModifiedBy)
            .WithMany()
            .HasForeignKey(oi => oi.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
