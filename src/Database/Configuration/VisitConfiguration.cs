using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Visit entity.
/// Defines cascade delete behavior: when a Visit is deleted, all related VisitItems are deleted.
/// </summary>
public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        // Cascade: Deleting a Visit deletes all its VisitItems
        builder
            .HasMany(v => v.Items)
            .WithOne(vi => vi.Visit)
            .HasForeignKey(vi => vi.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: Deleting an Appointment is prevented if Visits exist
        builder
            .HasOne(v => v.Appointment)
            .WithMany(a => a.Visits)
            .HasForeignKey(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: Deleting a Person is prevented if Visits exist
        builder
            .HasOne(v => v.Person)
            .WithMany()
            .HasForeignKey(v => v.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

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
