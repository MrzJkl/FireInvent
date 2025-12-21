using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for ItemAssignmentHistoryService.
/// These tests focus on business logic (validation, conflict checking for overlapping assignments) and data persistence.
/// </summary>
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

        var model = TestDataFactory.CreateAssignmentModel(Guid.NewGuid(), person.Id);

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

        var model = TestDataFactory.CreateAssignmentModel(itemId, Guid.NewGuid());

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
        var model = TestDataFactory.CreateAssignmentModel(itemId, personId, assignedById: nonExistingUserId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
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
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-5),
            assignedUntil: null);
        context.ItemAssignmentHistories.Add(existingAssignment);
        await context.SaveChangesAsync();

        // Attempting to create an overlapping assignment
        var model = TestDataFactory.CreateAssignmentModel(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow);

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
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-10),
            assignedUntil: DateTimeOffset.UtcNow.AddDays(-5));
        context.ItemAssignmentHistories.Add(existingAssignment);
        await context.SaveChangesAsync();

        // Non-overlapping assignment
        var model = TestDataFactory.CreateAssignmentModel(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow);

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
    public async Task GetAssignmentsForItemAsync_WithNonExistingItem_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAssignmentsForItemAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAssignmentsForItemAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, _) = await SetupBasicDataAsync(context);

        // Act
        var result = await service.GetAssignmentsForItemAsync(itemId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAssignmentsForPersonAsync_WithNonExistingPerson_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAssignmentsForPersonAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAssignmentsForPersonAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, _, personId) = await SetupBasicDataAsync(context);

        // Act
        var result = await service.GetAssignmentsForPersonAsync(personId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAssignmentsForPersonAsync_WithMultipleAssignments_ShouldReturnOrderedByAssignedFromDescending()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment1 = TestDataFactory.CreateAssignment(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-20),
            assignedUntil: DateTimeOffset.UtcNow.AddDays(-15));
        var assignment2 = TestDataFactory.CreateAssignment(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-10),
            assignedUntil: DateTimeOffset.UtcNow.AddDays(-5));
        var assignment3 = TestDataFactory.CreateAssignment(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow);

        context.ItemAssignmentHistories.AddRange(assignment1, assignment2, assignment3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAssignmentsForPersonAsync(personId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(assignment3.Id, result[0].Id);
        Assert.Equal(assignment2.Id, result[1].Id);
        Assert.Equal(assignment1.Id, result[2].Id);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);

        // Act
        var result = await service.GetAllAssignmentsAsync();

        // Assert
        Assert.Empty(result);
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

        var updateModel = TestDataFactory.CreateAssignmentModel(itemId, personId);

        // Act
        var result = await service.UpdateAssignmentAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithNonExistingItem_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(itemId, personId);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateAssignmentModel(Guid.NewGuid(), personId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAssignmentAsync(assignment.Id, updateModel));
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithNonExistingPerson_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(itemId, personId);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateAssignmentModel(itemId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAssignmentAsync(assignment.Id, updateModel));
    }

    [Fact]
    public async Task UpdateAssignmentAsync_WithNonExistingUser_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(itemId, personId);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var nonExistingUserId = Guid.NewGuid();
        var updateModel = TestDataFactory.CreateAssignmentModel(itemId, personId, assignedById: nonExistingUserId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAssignmentAsync(assignment.Id, updateModel));
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
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-10),
            assignedUntil: null);
        var assignmentToUpdate = TestDataFactory.CreateAssignment(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-20),
            assignedUntil: DateTimeOffset.UtcNow.AddDays(-15));
        context.ItemAssignmentHistories.AddRange(existingAssignment, assignmentToUpdate);
        await context.SaveChangesAsync();

        // Try to update to an overlapping date range
        var updateModel = TestDataFactory.CreateAssignmentModel(
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-5),
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
            itemId, personId,
            assignedFrom: DateTimeOffset.UtcNow.AddDays(-10),
            assignedUntil: null);
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var newEndDate = DateTimeOffset.UtcNow;
        var updateModel = TestDataFactory.CreateAssignmentModel(
            itemId, personId,
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
    public async Task DeleteAssignmentAsync_WithExistingId_ShouldDeleteAssignment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = CreateMockUserService();
        var service = new ItemAssignmentHistoryService(context, _mapper, mockUserService);
        var (_, itemId, personId) = await SetupBasicDataAsync(context);

        var assignment = TestDataFactory.CreateAssignment(itemId, personId);
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
