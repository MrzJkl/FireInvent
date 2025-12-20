using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for DepartmentService.
/// These tests focus on CRUD operations and duplicate name conflict detection.
/// </summary>
public class DepartmentServiceTests
{
    private readonly DepartmentMapper _mapper = new();

    [Fact]
    public async Task CreateDepartmentAsync_WithValidModel_ShouldCreateDepartment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var model = TestDataFactory.CreateDepartmentModel("Fire Brigade A", "Main fire department");

        // Act
        var result = await service.CreateDepartmentAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var existingDepartment = TestDataFactory.CreateDepartment(name: "Existing Department");
        context.Departments.Add(existingDepartment);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateDepartmentModel("Existing Department");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateDepartmentAsync(model));
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ShouldReturnAllDepartments_OrderedByName()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        context.Departments.AddRange(
            TestDataFactory.CreateDepartment(name: "Charlie"),
            TestDataFactory.CreateDepartment(name: "Alpha"),
            TestDataFactory.CreateDepartment(name: "Bravo")
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllDepartmentsAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alpha", result[0].Name);
        Assert.Equal("Bravo", result[1].Name);
        Assert.Equal("Charlie", result[2].Name);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.GetAllDepartmentsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_WithExistingId_ShouldReturnDepartment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var department = TestDataFactory.CreateDepartment(name: "Test Department");
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDepartmentByIdAsync(department.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(department.Id, result.Id);
        Assert.Equal(department.Name, result.Name);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.GetDepartmentByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithExistingId_ShouldUpdateDepartment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var department = TestDataFactory.CreateDepartment(name: "Original Name");
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateDepartmentModel("Updated Name", "Updated Description");

        // Act
        var result = await service.UpdateDepartmentAsync(department.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Departments.FindAsync(department.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var updateModel = TestDataFactory.CreateDepartmentModel("New Name");

        // Act
        var result = await service.UpdateDepartmentAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var existingDepartment = TestDataFactory.CreateDepartment(name: "Existing Name");
        var departmentToUpdate = TestDataFactory.CreateDepartment(name: "Original Name");
        context.Departments.AddRange(existingDepartment, departmentToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateDepartmentModel("Existing Name");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateDepartmentAsync(departmentToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task UpdateDepartmentAsync_WithSameName_ShouldSucceed()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var department = TestDataFactory.CreateDepartment(name: "Test Name");
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateDepartmentModel("Test Name", "New Description");

        // Act
        var result = await service.UpdateDepartmentAsync(department.Id, updateModel);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithExistingId_ShouldDeleteDepartment()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);
        var department = TestDataFactory.CreateDepartment();
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteDepartmentAsync(department.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Departments.FindAsync(department.Id));
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        // Act
        var result = await service.DeleteDepartmentAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
