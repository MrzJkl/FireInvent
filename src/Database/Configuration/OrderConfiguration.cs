using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Order entity.
/// Defines cascade delete behavior: when an Order is deleted, all related OrderItems are deleted.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Cascade: Deleting an Order deletes all its OrderItems
        builder
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(o => o.CreatedBy)
            .WithMany()
            .HasForeignKey(o => o.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(o => o.ModifiedBy)
            .WithMany()
            .HasForeignKey(o => o.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
