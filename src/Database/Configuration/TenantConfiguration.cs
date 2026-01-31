using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Tenant entity.
/// Defines base behavior for tenant-scoped entities with user audit trails.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(t => t.ModifiedBy)
            .WithMany()
            .HasForeignKey(t => t.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
