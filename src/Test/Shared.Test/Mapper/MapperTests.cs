using FireInvent.Contract;
using FireInvent.Database.Models;
using FireInvent.Shared.Mapper;

namespace FireInvent.Test.Shared.Mapper;

/// <summary>
/// Unit tests for Mapper classes.
/// These tests verify that mappers correctly transform entities to models and vice versa.
/// </summary>
public class MapperTests
{
    [Fact]
    public void DepartmentMapper_MapCreateOrUpdateDepartmentModelToDepartment_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var model = TestDataFactory.CreateDepartmentModel("Fire Brigade A", "Main fire department");

        // Act
        var result = mapper.MapCreateOrUpdateDepartmentModelToDepartment(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public void DepartmentMapper_MapDepartmentToDepartmentModel_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var department = TestDataFactory.CreateDepartment(name: "Fire Brigade A", description: "Main fire department");

        // Act
        var result = mapper.MapDepartmentToDepartmentModel(department);

        // Assert
        Assert.Equal(department.Id, result.Id);
        Assert.Equal(department.Name, result.Name);
        Assert.Equal(department.Description, result.Description);
    }

    [Fact]
    public void DepartmentMapper_MapDepartmentsToDepartmentModels_ShouldMapAllItems()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var departments = new List<Department>
        {
            TestDataFactory.CreateDepartment(name: "Dept A"),
            TestDataFactory.CreateDepartment(name: "Dept B"),
            TestDataFactory.CreateDepartment(name: "Dept C")
        };

        // Act
        var result = mapper.MapDepartmentsToDepartmentModels(departments);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Dept A", result[0].Name);
        Assert.Equal("Dept B", result[1].Name);
        Assert.Equal("Dept C", result[2].Name);
    }

    [Fact]
    public void ProductTypeMapper_MapCreateOrUpdateProductTypeModelToProductType_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new ProductTypeMapper();
        var model = TestDataFactory.CreateProductTypeModel("Helmet", "Head protection");

        // Act
        var result = mapper.MapCreateOrUpdateProductTypeModelToProductType(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public void StorageLocationMapper_MapCreateOrUpdateStorageLocationModelToStorageLocation_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new StorageLocationMapper();
        var model = TestDataFactory.CreateStorageLocationModel("Warehouse A", "Main storage");

        // Act
        var result = mapper.MapCreateOrUpdateStorageLocationModelToStorageLocation(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Remarks, result.Remarks);
    }

    [Fact]
    public void PersonMapper_MapCreateOrUpdatePersonModelToPerson_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new PersonMapper();
        var model = TestDataFactory.CreatePersonModel("John", "Doe", "EXT001");

        // Act
        var result = mapper.MapCreateOrUpdatePersonModelToPerson(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.FirstName, result.FirstName);
        Assert.Equal(model.LastName, result.LastName);
        Assert.Equal(model.ExternalId, result.ExternalId);
    }

    [Fact]
    public void PersonMapper_MapPersonToPersonModel_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new PersonMapper();
        var person = TestDataFactory.CreatePerson(firstName: "John", lastName: "Doe", externalId: "EXT001");
        person.Remarks = "Test remarks";
        person.EMail = "test@example.com";

        // Act
        var result = mapper.MapPersonToPersonModel(person);

        // Assert
        Assert.Equal(person.Id, result.Id);
        Assert.Equal(person.FirstName, result.FirstName);
        Assert.Equal(person.LastName, result.LastName);
        Assert.Equal(person.ExternalId, result.ExternalId);
        Assert.Equal(person.Remarks, result.Remarks);
        Assert.Equal(person.EMail, result.EMail);
    }

    [Fact]
    public void MaintenanceTypeMapper_MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new MaintenanceTypeMapper();
        var model = TestDataFactory.CreateMaintenanceTypeModel("Inspection", "Annual inspection");

        // Act
        var result = mapper.MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public void ItemMapper_MapCreateOrUpdateItemModelToItem_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new ItemMapper();
        var variantId = Guid.NewGuid();
        var purchaseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var model = TestDataFactory.CreateItemModel(variantId, ItemCondition.New, purchaseDate, "ITEM-001");

        // Act
        var result = mapper.MapCreateOrUpdateItemModelToItem(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.VariantId, result.VariantId);
        Assert.Equal(model.Condition, result.Condition);
        Assert.Equal(model.Identifier, result.Identifier);
        Assert.Equal(model.PurchaseDate, result.PurchaseDate);
    }

    [Fact]
    public void VariantMapper_MapCreateOrUpdateVariantModelToVariant_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new VariantMapper();
        var productId = Guid.NewGuid();
        var model = TestDataFactory.CreateVariantModel(productId, "Size L", "Large size");

        // Act
        var result = mapper.MapCreateOrUpdateVariantModelToVariant(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.ProductId, result.ProductId);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.AdditionalSpecs, result.AdditionalSpecs);
    }

    [Fact]
    public void ProductMapper_MapCreateOrUpdateProductModelToProduct_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new ProductMapper();
        var typeId = Guid.NewGuid();
        var model = TestDataFactory.CreateProductModel(typeId, Guid.NewGuid(), "BrandA", "High quality helmet");

        // Act
        var result = mapper.MapCreateOrUpdateProductModelToProduct(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.TypeId, result.TypeId);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.ManufacturerId, result.ManufacturerId);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public void OrderMapper_MapCreateOrUpdateOrderModelToOrder_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new OrderMapper();
        var orderDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var model = TestDataFactory.CreateOrderModel(orderDate, OrderStatus.Draft, "ORD-001");

        // Act
        var result = mapper.MapCreateOrUpdateOrderModelToOrder(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.OrderDate, result.OrderDate);
        Assert.Equal(model.Status, result.Status);
        Assert.Equal(model.OrderIdentifier, result.OrderIdentifier);
    }

    [Fact]
    public void MaintenanceMapper_MapCreateOrUpdateMaintenanceModelToMaintenance_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new MaintenanceMapper();
        var itemId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var performedAt = DateTimeOffset.UtcNow;
        var model = TestDataFactory.CreateMaintenanceModel(itemId, typeId, performedAt, remarks: "Test remarks");

        // Act
        var result = mapper.MapCreateOrUpdateMaintenanceModelToMaintenance(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.ItemId, result.ItemId);
        Assert.Equal(model.TypeId, result.TypeId);
        Assert.Equal(model.PerformedAt, result.PerformedAt);
        Assert.Equal(model.Remarks, result.Remarks);
    }

    [Fact]
    public void ItemAssignmentHistoryMapper_MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory_ShouldMapAllProperties()
    {
        // Arrange
        var mapper = new ItemAssignmentHistoryMapper();
        var itemId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var assignedFrom = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var assignedUntil = DateOnly.FromDateTime(DateTime.UtcNow);
        var model = TestDataFactory.CreateAssignmentModel(itemId, personId, assignedFrom, assignedUntil);

        // Act
        var result = mapper.MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.ItemId, result.ItemId);
        Assert.Equal(model.PersonId, result.PersonId);
        Assert.Equal(model.AssignedFrom, result.AssignedFrom);
        Assert.Equal(model.AssignedUntil, result.AssignedUntil);
    }
}
