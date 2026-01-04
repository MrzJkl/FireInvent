using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared.Services;

public class ItemAssignmentHistoryServiceTests
{
    private readonly ItemAssignmentHistoryMapper _mapper = new();

    private MockUserService CreateMockUserService()
    {
        var mockUserService = new MockUserService();
        mockUserService.AddUser(TestDataFactory.DefaultTestUserId);
        return mockUserService;
    }

    private async Task<(Guid VariantId, Guid ItemId, Guid PersonId)> SetupBasicDataAsync(Database.AppDbContext context)
    {
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id, name: "Size L");
        variant.Product = product;
        var item = TestDataFactory.CreateItem(variant.Id);
        item.Variant = variant;
        var person = TestDataFactory.CreatePerson(firstName: "John", lastName: "Doe");

        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        context.Items.Add(item);
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        return (variant.Id, item.Id, person.Id);
    }

    private async Task<(Guid VariantId, Guid ItemId, Guid StorageLocationId)> SetupBasicDataWithStorageLocationAsync(Database.AppDbContext context)
    {
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id, name: "Size L");
        variant.Product = product;
        var item = TestDataFactory.CreateItem(variant.Id);
        item.Variant = variant;
        var storageLocation = TestDataFactory.CreateStorageLocation(name: "Warehouse A");

        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        context.Items.Add(item);
        context.StorageLocations.Add(storageLocation);
        await context.SaveChangesAsync();

        return (variant.Id, item.Id, storageLocation.Id);
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithNonExistingItem_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var person = TestDataFactory.CreatePerson(firstName: "John", lastName: "Doe");
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateAssignmentModel(Guid.NewGuid(), personId: person.Id);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithNonExistingPerson_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, _) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateAssignmentModel(itemId, personId: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithNonExistingStorageLocation_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, _) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateAssignmentModel(itemId, storageLocationId: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithNonExistingUser_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var nonExistingUserId = Guid.NewGuid();
        var model = TestDataFactory.CreateAssignmentModel(itemId, personId: personId, assignedById: nonExistingUserId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithBothPersonAndStorageLocation_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);
        var storageLocation = TestDataFactory.CreateStorageLocation(name: "Test Location");
        context.StorageLocations.Add(storageLocation);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateAssignmentModel(itemId, personId: personId, storageLocationId: storageLocation.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
        Assert.Contains("either a Person or a StorageLocation", exception.Message);
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithNeitherPersonNorStorageLocation_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, _) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateAssignmentModel(itemId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
        Assert.Contains("Either PersonId or StorageLocationId must be set", exception.Message);
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithOverlappingAssignment_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var existingAssignment = TestDataFactory.CreateAssignment(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            assignedUntil: null);
        context.ItemAssignmentHistories.Add(existingAssignment);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateAssignmentModel(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithNonOverlappingAssignment_ShouldPersistAssignment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var existingAssignment = TestDataFactory.CreateAssignment(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            assignedUntil: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)));
        context.ItemAssignmentHistories.Add(existingAssignment);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateAssignmentModel(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var countBefore = await context.ItemAssignmentHistories.CountAsync();
        var result = await service.CreateAssignmentAsync(model);

        // Assert
        var countAfter = await context.ItemAssignmentHistories.CountAsync();
        Assert.Equal(countBefore + 1, countAfter);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(TestDataFactory.DefaultTestUserId, result.AssignedById);
    }

    [Fact]
    public async Task CreateAssignmentAsync_WithStorageLocation_ShouldPersistAssignment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, storageLocationId) = await SetupBasicDataWithStorageLocationAsync(context);

        var model = TestDataFactory.CreateAssignmentModel(
            itemId, storageLocationId: storageLocationId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var result = await service.CreateAssignmentAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(storageLocationId, result.StorageLocationId);
        Assert.Null(result.PersonId);
    }

    [Fact]
    public async Task GetAssignmentsForItemAsync_WithNonExistingItem_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAssignmentsForItemAsync(Guid.NewGuid(), query, CancellationToken.None));
    }

    [Fact]
    public async Task GetAssignmentsForItemAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, _) = await SetupBasicDataAsync(context);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAssignmentsForItemAsync(itemId, query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAssignmentsForPersonAsync_WithNonExistingPerson_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAssignmentsForPersonAsync(Guid.NewGuid(), query, CancellationToken.None));
    }

    [Fact]
    public async Task GetAssignmentsForPersonAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, _, personId) = await SetupBasicDataAsync(context);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAssignmentsForPersonAsync(personId, query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAssignmentsForStorageLocationAsync_WithNonExistingLocation_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAssignmentsForStorageLocationAsync(Guid.NewGuid(), query, CancellationToken.None));
    }

    [Fact]
    public async Task GetAssignmentsForStorageLocationAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, _, storageLocationId) = await SetupBasicDataWithStorageLocationAsync(context);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAssignmentsForStorageLocationAsync(storageLocationId, query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAssignmentsAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithMultipleAssignments_ShouldReturnAll()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);
        var storageLocation = TestDataFactory.CreateStorageLocation(name: "Test Location");
        context.StorageLocations.Add(storageLocation);
        await context.SaveChangesAsync();

        var assignment1 = TestDataFactory.CreateAssignment(itemId, personId: personId);
        var assignment2 = TestDataFactory.CreateAssignment(itemId, storageLocationId: storageLocation.Id,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        context.ItemAssignmentHistories.AddRange(assignment1, assignment2);
        await context.SaveChangesAsync();
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllAssignmentsAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_WithExistingId_ShouldReturnAssignment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(itemId, personId: personId);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAssignmentByIdAsync(assignment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assignment.Id, result.Id);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);

        // Act
        var result = await service.GetAssignmentByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateAssignmentModel(itemId, personId: personId);

        // Act
        var result = await service.UpdateAssignmentAsync(Guid.NewGuid(), model);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithOverlappingAssignment_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var existingAssignment = TestDataFactory.CreateAssignment(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            assignedUntil: null);
        var assignmentToUpdate = TestDataFactory.CreateAssignment(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
            assignedUntil: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)));
        context.ItemAssignmentHistories.AddRange(existingAssignment, assignmentToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateAssignmentModel(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            assignedUntil: null);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAssignmentAsync(assignmentToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithValidModel_ShouldUpdateAssignment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(
            itemId, personId: personId,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            assignedUntil: null);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var newEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var updateModel = TestDataFactory.CreateAssignmentModel(
            itemId, personId: personId,
            assignedFrom: assignment.AssignedFrom,
            assignedUntil: newEndDate);

        // Act
        var result = await service.UpdateAssignmentAsync(assignment.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.ItemAssignmentHistories.FindAsync(assignment.Id);
        Assert.NotNull(updated);
        Assert.NotNull(updated.AssignedUntil);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithBothPersonAndStorageLocation_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);
        var storageLocation = TestDataFactory.CreateStorageLocation(name: "Test Location");
        context.StorageLocations.Add(storageLocation);
        await context.SaveChangesAsync();

        var assignment = TestDataFactory.CreateAssignment(itemId, personId: personId);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateAssignmentModel(itemId, personId: personId, storageLocationId: storageLocation.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAssignmentAsync(assignment.Id, updateModel));
        Assert.Contains("either a Person or a StorageLocation", exception.Message);
    }

    [Fact]
    public async Task DeleteAssignmentAsync_WithExistingId_ShouldDeleteAssignment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(itemId, personId: personId);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAssignmentAsync(assignment.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.ItemAssignmentHistories.FindAsync(assignment.Id));
    }

    [Fact]
    public async Task DeleteAssignmentAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);

        // Act
        var result = await service.DeleteAssignmentAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
