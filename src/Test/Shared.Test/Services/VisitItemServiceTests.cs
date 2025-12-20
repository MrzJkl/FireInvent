using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for VisitItemService.
/// </summary>
public class VisitItemServiceTests
{
    private readonly VisitMapper _mapper = new();

    [Fact]
    public async Task GetVisitItemByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);

        // Act
        var result = await service.GetVisitItemByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllVisitItemsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);

        // Act
        var result = await service.GetAllVisitItemsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateVisitItemAsync_WithValidModel_ShouldCreateVisitItem()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product = TestDataFactory.CreateProduct(
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVisitItemModel(
            visitId: visit.Id,
            productId: product.Id,
            quantity: 5);

        // Act
        var result = await service.CreateVisitItemAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var savedVisitItem = await context.VisitItems.FindAsync(result.Id);
        Assert.NotNull(savedVisitItem);
        Assert.Equal(visit.Id, savedVisitItem.VisitId);
        Assert.Equal(product.Id, savedVisitItem.ProductId);
        Assert.Equal(5, savedVisitItem.Quantity);
    }

    [Fact]
    public async Task GetVisitItemByIdAsync_WithExistingId_ShouldReturnVisitItem()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product = TestDataFactory.CreateProduct(
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var visitItem = TestDataFactory.CreateVisitItem(
            visitId: visit.Id,
            productId: product.Id,
            quantity: 3);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        context.VisitItems.Add(visitItem);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetVisitItemByIdAsync(visitItem.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(visitItem.Id, result.Id);
        Assert.Equal(visit.Id, result.VisitId);
        Assert.Equal(product.Id, result.ProductId);
        Assert.Equal(3, result.Quantity);
    }

    [Fact]
    public async Task GetAllVisitItemsAsync_WithMultipleVisitItems_ShouldReturnAllVisitItems()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product1 = TestDataFactory.CreateProduct(
            name: "Product 1",
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var product2 = TestDataFactory.CreateProduct(
            name: "Product 2",
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var visitItem1 = TestDataFactory.CreateVisitItem(
            visitId: visit.Id,
            productId: product1.Id,
            quantity: 2);
        var visitItem2 = TestDataFactory.CreateVisitItem(
            visitId: visit.Id,
            productId: product2.Id,
            quantity: 3);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.AddRange(product1, product2);
        context.VisitItems.AddRange(visitItem1, visitItem2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllVisitItemsAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetVisitItemsByVisitIdAsync_ShouldReturnOnlyItemsForSpecificVisit()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment1 = TestDataFactory.CreateAppointment();
        var appointment2 = TestDataFactory.CreateAppointment();
        var visit1 = TestDataFactory.CreateVisit(
            appointmentId: appointment1.Id,
            personId: person.Id);
        var visit2 = TestDataFactory.CreateVisit(
            appointmentId: appointment2.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product = TestDataFactory.CreateProduct(
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var visitItem1 = TestDataFactory.CreateVisitItem(
            visitId: visit1.Id,
            productId: product.Id);
        var visitItem2 = TestDataFactory.CreateVisitItem(
            visitId: visit1.Id,
            productId: product.Id);
        var visitItem3 = TestDataFactory.CreateVisitItem(
            visitId: visit2.Id,
            productId: product.Id);
        
        context.Persons.Add(person);
        context.Appointments.AddRange(appointment1, appointment2);
        context.Visits.AddRange(visit1, visit2);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        context.VisitItems.AddRange(visitItem1, visitItem2, visitItem3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetVisitItemsByVisitIdAsync(visit1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal(visit1.Id, item.VisitId));
    }

    [Fact]
    public async Task UpdateVisitItemAsync_WithValidModel_ShouldUpdateVisitItem()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product = TestDataFactory.CreateProduct(
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var visitItem = TestDataFactory.CreateVisitItem(
            visitId: visit.Id,
            productId: product.Id,
            quantity: 2);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        context.VisitItems.Add(visitItem);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVisitItemModel(
            visitId: visit.Id,
            productId: product.Id,
            quantity: 10); // Update quantity

        // Act
        var result = await service.UpdateVisitItemAsync(visitItem.Id, updateModel);

        // Assert
        Assert.True(result);
        var updatedVisitItem = await context.VisitItems.FindAsync(visitItem.Id);
        Assert.NotNull(updatedVisitItem);
        Assert.Equal(10, updatedVisitItem.Quantity);
    }

    [Fact]
    public async Task UpdateVisitItemAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var updateModel = TestDataFactory.CreateVisitItemModel();

        // Act
        var result = await service.UpdateVisitItemAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteVisitItemAsync_WithValidId_ShouldDeleteVisitItem()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product = TestDataFactory.CreateProduct(
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var visitItem = TestDataFactory.CreateVisitItem(
            visitId: visit.Id,
            productId: product.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        context.VisitItems.Add(visitItem);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteVisitItemAsync(visitItem.Id);

        // Assert
        Assert.True(result);
        var deletedVisitItem = await context.VisitItems.FindAsync(visitItem.Id);
        Assert.Null(deletedVisitItem);
    }

    [Fact]
    public async Task DeleteVisitItemAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);

        // Act
        var result = await service.DeleteVisitItemAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateVisitItemAsync_WithMultipleItemsForSameVisit_ShouldSucceed()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VisitItemService(context, _mapper);
        
        var person = TestDataFactory.CreatePerson();
        var appointment = TestDataFactory.CreateAppointment();
        var visit = TestDataFactory.CreateVisit(
            appointmentId: appointment.Id,
            personId: person.Id);
        var manufacturer = TestDataFactory.CreateManufacturer();
        var productType = TestDataFactory.CreateProductType();
        var product1 = TestDataFactory.CreateProduct(
            name: "Product 1",
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        var product2 = TestDataFactory.CreateProduct(
            name: "Product 2",
            typeId: productType.Id,
            manufacturerId: manufacturer.Id);
        
        context.Persons.Add(person);
        context.Appointments.Add(appointment);
        context.Visits.Add(visit);
        context.Manufacturers.Add(manufacturer);
        context.ProductTypes.Add(productType);
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var model1 = TestDataFactory.CreateVisitItemModel(
            visitId: visit.Id,
            productId: product1.Id,
            quantity: 2);
        var model2 = TestDataFactory.CreateVisitItemModel(
            visitId: visit.Id,
            productId: product2.Id,
            quantity: 3);

        // Act
        var result1 = await service.CreateVisitItemAsync(model1);
        var result2 = await service.CreateVisitItemAsync(model2);

        // Assert
        Assert.NotEqual(Guid.Empty, result1.Id);
        Assert.NotEqual(Guid.Empty, result2.Id);
        var items = await service.GetVisitItemsByVisitIdAsync(visit.Id);
        Assert.Equal(2, items.Count);
    }
}
