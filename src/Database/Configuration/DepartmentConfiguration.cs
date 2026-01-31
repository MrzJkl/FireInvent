using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Department entity.
/// Defines restrict delete behavior: Department is protected and cannot be deleted if Persons are assigned to it.
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        // Restrict: Deleting a Department is prevented if Persons are assigned to it
        // For many-to-many, configure the join entity or use fluent API with both sides
        builder
            .HasMany(d => d.Persons)
            .WithMany(p => p.Departments);

        // SetNull: Deleting a User sets CreatedBy to null
        builder
            .HasOne(d => d.CreatedBy)
            .WithMany()
            .HasForeignKey(d => d.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(d => d.ModifiedBy)
            .WithMany()
            .HasForeignKey(d => d.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
