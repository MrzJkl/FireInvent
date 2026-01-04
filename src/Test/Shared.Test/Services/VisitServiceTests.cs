using FireInvent.Contract.Exceptions;
using FireInvent.Contract;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for VisitService.
/// These tests focus on business logic (unique constraint validation) and data persistence.
/// </summary>
public class VisitServiceTests
{
    private readonly VisitMapper _mapper = new();

    [Fact]
    public async Task GetVisitByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);

        // Act
        var result = await service.GetVisitByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllVisitsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllVisitsAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task CreateVisitAsync_WithValidModel_ShouldCreateVisit()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVisitModel(
            appointmentId: appointment.Id,
            personId: person.Id);

        // Act
        var result = await service.CreateVisitAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var savedVisit = await context.Visits.FindAsync(result.Id);
        Assert.NotNull(savedVisit);
        Assert.Equal(appointment.Id, savedVisit.AppointmentId);
        Assert.Equal(person.Id, savedVisit.PersonId);
    }

    [Fact]
    public async Task CreateVisitAsync_WithNonExistingAppointment_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVisitModel(
            appointmentId: Guid.NewGuid(), // Non-existing appointment
            personId: person.Id);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateVisitAsync(model));
    }

    [Fact]
    public async Task CreateVisitAsync_WithNonExistingPerson_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var appointment = TestDataFactory.CreateAppointment();
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVisitModel(
            appointmentId: appointment.Id,
            personId: Guid.NewGuid()); // Non-existing person

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateVisitAsync(model));
    }

    [Fact]
    public async Task CreateVisitAsync_WithDuplicateAppointmentAndPerson_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var existingVisit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(existingVisit);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVisitModel(
            appointmentId: appointment.Id,
            personId: person.Id); // Same combination as existing visit

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateVisitAsync(model));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task CreateVisitAsync_WithSamePersonDifferentAppointment_ShouldSucceed()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment1 = TestDataFactory.CreateAppointment();
        var appointment2 = TestDataFactory.CreateAppointment();
        var existingVisit = TestDataFactory.CreateVisit(
            appointmentId: appointment1.Id,
            personId: person.Id);
        
        context.Persons.Add(person);
        context.Appointments.AddRange(appointment1, appointment2);
        context.Visits.Add(existingVisit);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVisitModel(
            appointmentId: appointment2.Id, // Different appointment
            personId: person.Id);

        // Act
        var result = await service.CreateVisitAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var savedVisit = await context.Visits.FindAsync(result.Id);
        Assert.NotNull(savedVisit);
    }

    [Fact]
    public async Task GetVisitByIdAsync_WithExistingId_ShouldReturnVisit()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetVisitByIdAsync(visit.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(visit.Id, result.Id);
        Assert.Equal(appointment.Id, result.AppointmentId);
        Assert.Equal(person.Id, result.PersonId);
    }

    [Fact]
    public async Task GetAllVisitsAsync_WithMultipleVisits_ShouldReturnAllVisits()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person1 = TestDataFactory.CreatePerson(firstName: "John");
        var person2 = TestDataFactory.CreatePerson(firstName: "Jane");
        var appointment = TestDataFactory.CreateAppointment();
        var visit1 = TestDataFactory.CreateVisit(appointmentId: appointment.Id, personId: person1.Id);
        var visit2 = TestDataFactory.CreateVisit(appointmentId: appointment.Id, personId: person2.Id);
        
        context.Persons.AddRange(person1, person2);
        context.Appointments.Add(appointment);
        context.Visits.AddRange(visit1, visit2);
        await context.SaveChangesAsync();
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllVisitsAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task UpdateVisitAsync_WithValidModel_ShouldUpdateVisit()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person1 = TestDataFactory.CreatePerson(firstName: "John");
        var person2 = TestDataFactory.CreatePerson(firstName: "Jane");
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person1.Id);
        
        context.Persons.AddRange(person1, person2);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVisitModel(
            appointmentId: appointment.Id,
            personId: person2.Id); // Change to different person

        // Act
        var result = await service.UpdateVisitAsync(visit.Id, updateModel);

        // Assert
        Assert.True(result);
        var updatedVisit = await context.Visits.FindAsync(visit.Id);
        Assert.NotNull(updatedVisit);
        Assert.Equal(person2.Id, updatedVisit.PersonId);
    }

    [Fact]
    public async Task UpdateVisitAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVisitModel(
            appointmentId: appointment.Id,
            personId: person.Id);

        // Act
        var result = await service.UpdateVisitAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateVisitAsync_WithDuplicateAppointmentAndPerson_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person1 = TestDataFactory.CreatePerson(firstName: "John");
        var person2 = TestDataFactory.CreatePerson(firstName: "Jane");
        var appointment = TestDataFactory.CreateAppointment();
        var visit1 = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person1.Id);
        var visit2 = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person2.Id);
        
        context.Persons.AddRange(person1, person2);
        context.Appointments.Add(appointment);
        context.Visits.AddRange(visit1, visit2);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVisitModel(
            appointmentId: appointment.Id,
            personId: person1.Id); // Try to change visit2 to same person as visit1

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.UpdateVisitAsync(visit2.Id, updateModel));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task DeleteVisitAsync_WithValidId_ShouldDeleteVisit()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteVisitAsync(visit.Id);

        // Assert
        Assert.True(result);
        var deletedVisit = await context.Visits.FindAsync(visit.Id);
        Assert.Null(deletedVisit);
    }

    [Fact]
    public async Task DeleteVisitAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);

        // Act
        var result = await service.DeleteVisitAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteVisitAsync_WithVisitItems_ShouldDeleteVisitAndItems()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product = TestDataFactory.CreateProduct(
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var visitItem = TestDataFactory.CreateVisitItem(
            visitId: visit.Id,
            productId: product.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        context.Visits.Add(visit);
        context.VisitItems.Add(visitItem);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteVisitAsync(visit.Id);

        // Assert
        Assert.True(result);
        var deletedVisit = await context.Visits.FindAsync(visit.Id);
        var deletedVisitItem = await context.VisitItems.FindAsync(visitItem.Id);
        Assert.Null(deletedVisit);
        Assert.Null(deletedVisitItem); // Should be cascade deleted
    }
}
