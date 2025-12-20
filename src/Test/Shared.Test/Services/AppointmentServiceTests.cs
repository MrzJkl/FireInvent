using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for AppointmentService.
/// </summary>
public class AppointmentServiceTests
{
    private readonly AppointmentMapper _mapper = new();

    [Fact]
    public async Task GetAppointmentByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);

        // Act
        var result = await service.GetAppointmentByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);

        // Act
        var result = await service.GetAllAppointmentsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithValidModel_ShouldCreateAppointment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        var scheduledAt = DateTimeOffset.UtcNow.AddDays(7);
        var model = TestDataFactory.CreateAppointmentModel(
            scheduledAt: scheduledAt,
            description: "Annual team meeting");

        // Act
        var result = await service.CreateAppointmentAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var savedAppointment = await context.Appointments.FindAsync(result.Id);
        Assert.NotNull(savedAppointment);
        Assert.Equal(scheduledAt, savedAppointment.ScheduledAt);
        Assert.Equal("Annual team meeting", savedAppointment.Description);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_WithExistingId_ShouldReturnAppointment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        var appointment = TestDataFactory.CreateAppointment(
            scheduledAt: DateTimeOffset.UtcNow.AddDays(3),
            description: "Test Appointment");
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAppointmentByIdAsync(appointment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(appointment.Id, result.Id);
        Assert.Equal(appointment.ScheduledAt, result.ScheduledAt);
        Assert.Equal("Test Appointment", result.Description);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_WithMultipleAppointments_ShouldReturnAllAppointments()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        var appointment1 = TestDataFactory.CreateAppointment(scheduledAt: DateTimeOffset.UtcNow.AddDays(1));
        var appointment2 = TestDataFactory.CreateAppointment(scheduledAt: DateTimeOffset.UtcNow.AddDays(2));
        var appointment3 = TestDataFactory.CreateAppointment(scheduledAt: DateTimeOffset.UtcNow.AddDays(3));
        context.Appointments.AddRange(appointment1, appointment2, appointment3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllAppointmentsAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_WithValidModel_ShouldUpdateAppointment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        var appointment = TestDataFactory.CreateAppointment(description: "Original Description");
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateAppointmentModel(
            scheduledAt: DateTimeOffset.UtcNow.AddDays(10),
            description: "Updated Description");

        // Act
        var result = await service.UpdateAppointmentAsync(appointment.Id, updateModel);

        // Assert
        Assert.True(result);
        var updatedAppointment = await context.Appointments.FindAsync(appointment.Id);
        Assert.NotNull(updatedAppointment);
        Assert.Equal("Updated Description", updatedAppointment.Description);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        var updateModel = TestDataFactory.CreateAppointmentModel();

        // Act
        var result = await service.UpdateAppointmentAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAppointmentAsync_WithValidId_ShouldDeleteAppointment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        var appointment = TestDataFactory.CreateAppointment();
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAppointmentAsync(appointment.Id);

        // Assert
        Assert.True(result);
        var deletedAppointment = await context.Appointments.FindAsync(appointment.Id);
        Assert.Null(deletedAppointment);
    }

    [Fact]
    public async Task DeleteAppointmentAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);

        // Act
        var result = await service.DeleteAppointmentAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAppointmentAsync_WithVisits_ShouldDeleteAppointmentAndVisits()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new AppointmentService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(appointmentId: appointment.Id, personId: person.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAppointmentAsync(appointment.Id);

        // Assert
        Assert.True(result);
        var deletedAppointment = await context.Appointments.FindAsync(appointment.Id);
        var deletedVisit = await context.Visits.FindAsync(visit.Id);
        Assert.Null(deletedAppointment);
        Assert.Null(deletedVisit); // Should be cascade deleted
    }
}
