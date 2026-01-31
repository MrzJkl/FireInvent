using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Appointment entity.
/// Defines cascade delete behavior: when an Appointment is deleted, all related Visits are deleted.
/// </summary>
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        // Cascade: Deleting an Appointment deletes all its Visits
        builder
            .HasMany(a => a.Visits)
            .WithOne(v => v.Appointment)
            .HasForeignKey(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: Deleting a User sets CreatedBy to null (audit trail preserved)
        builder
            .HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: Deleting a User sets ModifiedBy to null
        builder
            .HasOne(a => a.ModifiedBy)
            .WithMany()
            .HasForeignKey(a => a.ModifiedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
