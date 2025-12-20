using FireInvent.Database.Models;
using FireInvent.Shared.Models;

namespace FireInvent.Test.Shared;

internal static partial class TestDataFactory
{
    // Appointment methods
    internal static CreateOrUpdateAppointmentModel CreateAppointmentModel(
        DateTimeOffset? scheduledAt = null,
        string? description = null)
        => new()
        {
            ScheduledAt = scheduledAt ?? DateTimeOffset.UtcNow.AddDays(1),
            Description = description
        };

    internal static Appointment CreateAppointment(
        Guid? id = null,
        DateTimeOffset? scheduledAt = null,
        string? description = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            TenantId = TestHelper.TestTenantId,
            ScheduledAt = scheduledAt ?? DateTimeOffset.UtcNow.AddDays(1),
            Description = description
        };

    // Visit methods
    internal static CreateOrUpdateVisitModel CreateVisitModel(
        Guid? appointmentId = null,
        Guid? personId = null)
        => new()
        {
            AppointmentId = appointmentId ?? Guid.NewGuid(),
            PersonId = personId ?? Guid.NewGuid()
        };

    internal static Visit CreateVisit(
        Guid? id = null,
        Guid? appointmentId = null,
        Guid? personId = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            TenantId = TestHelper.TestTenantId,
            AppointmentId = appointmentId ?? Guid.NewGuid(),
            PersonId = personId ?? Guid.NewGuid()
        };

    // VisitItem methods
    internal static CreateOrUpdateVisitItemModel CreateVisitItemModel(
        Guid? visitId = null,
        Guid? productId = null,
        int quantity = 1)
        => new()
        {
            VisitId = visitId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity
        };

    internal static VisitItem CreateVisitItem(
        Guid? id = null,
        Guid? visitId = null,
        Guid? productId = null,
        int quantity = 1)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            TenantId = TestHelper.TestTenantId,
            VisitId = visitId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            Quantity = quantity
        };
}
