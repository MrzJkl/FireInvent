﻿using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FireInvent.Database.Configuration;

/// <summary>
/// Configuration for the Person entity.
/// Defines cascade delete behavior: when a Person is deleted, their item assignment histories are deleted as well.
/// The actual items are preserved - only the assignment records are removed.
/// </summary>
public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        // Cascade: Deleting a Person deletes all their assignment histories, but preserves the items
        builder
            .HasMany(p => p.AssignedItems)
            .WithOne(ia => ia.Person)
            .HasForeignKey(ia => ia.PersonId)
            .OnDelete(DeleteBehavior.SetNull);

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
