using FireInvent.Contract;
using FireInvent.Database.Models;
using FireInvent.Shared.Models;

namespace FireInvent.Test.Shared;

/// <summary>
/// Factory class for creating test data instances.
/// </summary>
internal static class TestDataFactory
{
    // Department helpers
    internal static CreateOrUpdateDepartmentModel CreateDepartmentModel(string name = "Test Department", string? description = null)
        => new()
        {
            Name = name,
            Description = description
        };

    internal static Department CreateDepartment(Guid? id = null, string name = "Test Department", string? description = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description
        };

    // ProductType helpers
    internal static CreateOrUpdateProductTypeModel CreateProductTypeModel(string name = "Test ProductType", string? description = null)
        => new()
        {
            Name = name,
            Description = description
        };

    internal static ProductType CreateProductType(Guid? id = null, string name = "Test ProductType", string? description = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description
        };

    // MaintenanceType helpers
    internal static CreateOrUpdateMaintenanceTypeModel CreateMaintenanceTypeModel(string name = "Test MaintenanceType", string? description = null)
        => new()
        {
            Name = name,
            Description = description
        };

    internal static MaintenanceType CreateMaintenanceType(Guid? id = null, string name = "Test MaintenanceType", string? description = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description
        };

    // StorageLocation helpers
    internal static CreateOrUpdateStorageLocationModel CreateStorageLocationModel(string name = "Test Location", string? remarks = null)
        => new()
        {
            Name = name,
            Remarks = remarks
        };

    internal static StorageLocation CreateStorageLocation(Guid? id = null, string name = "Test Location", string? remarks = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Remarks = remarks
        };

    // Person helpers
    internal static CreateOrUpdatePersonModel CreatePersonModel(
        string firstName = "John",
        string lastName = "Doe",
        string? externalId = null,
        List<Guid>? departmentIds = null)
        => new()
        {
            FirstName = firstName,
            LastName = lastName,
            ExternalId = externalId,
            DepartmentIds = departmentIds ?? []
        };

    internal static Person CreatePerson(
        Guid? id = null,
        string firstName = "John",
        string lastName = "Doe",
        string? externalId = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            ExternalId = externalId
        };

    // Product helpers
    internal static CreateOrUpdateProductModel CreateProductModel(
        Guid typeId,
        Guid manufacturerId,
        string name = "Test Product",
        string? description = null)
        => new()
        {
            TypeId = typeId,
            Name = name,
            ManufacturerId = manufacturerId,
            Description = description
        };

    internal static Product CreateProduct(
        Guid typeId,
        Guid manufacturerId,
        Guid? id = null,
        string name = "Test Product",
        string manufacturer = "Test Manufacturer",
        string? description = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            TypeId = typeId,
            Name = name,
            ManufacturerId = manufacturerId,
            Description = description
        };

    // Variant helpers
    internal static CreateOrUpdateVariantModel CreateVariantModel(
        Guid productId,
        string name = "Test Variant",
        string? additionalSpecs = null)
        => new()
        {
            ProductId = productId,
            Name = name,
            AdditionalSpecs = additionalSpecs
        };

    internal static Variant CreateVariant(
        Guid productId,
        Guid? id = null,
        string name = "Test Variant",
        string? additionalSpecs = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            ProductId = productId,
            Name = name,
            AdditionalSpecs = additionalSpecs
        };

    // Item helpers
    internal static CreateOrUpdateItemModel CreateItemModel(
        Guid variantId,
        ItemCondition condition = ItemCondition.New,
        DateTimeOffset? purchaseDate = null,
        string? identifier = null,
        Guid? storageLocationId = null)
        => new()
        {
            VariantId = variantId,
            Condition = condition,
            PurchaseDate = purchaseDate ?? DateTimeOffset.UtcNow,
            Identifier = identifier,
            StorageLocationId = storageLocationId
        };

    internal static Item CreateItem(
        Guid variantId,
        Guid? id = null,
        ItemCondition condition = ItemCondition.New,
        DateTimeOffset? purchaseDate = null,
        string? identifier = null,
        Guid? storageLocationId = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            VariantId = variantId,
            Condition = condition,
            PurchaseDate = purchaseDate ?? DateTimeOffset.UtcNow,
            Identifier = identifier,
            StorageLocationId = storageLocationId
        };

    // ItemAssignmentHistory helpers
    internal static CreateOrUpdateItemAssignmentHistoryModel CreateAssignmentModel(
        Guid itemId,
        Guid personId,
        DateTimeOffset? assignedFrom = null,
        DateTimeOffset? assignedUntil = null,
        Guid? assignedById = null)
        => new()
        {
            ItemId = itemId,
            PersonId = personId,
            AssignedFrom = assignedFrom ?? DateTimeOffset.UtcNow,
            AssignedUntil = assignedUntil,
            AssignedById = assignedById
        };

    internal static ItemAssignmentHistory CreateAssignment(
        Guid itemId,
        Guid personId,
        Guid? id = null,
        DateTimeOffset? assignedFrom = null,
        DateTimeOffset? assignedUntil = null,
        Guid? assignedById = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            ItemId = itemId,
            PersonId = personId,
            AssignedFrom = assignedFrom ?? DateTimeOffset.UtcNow,
            AssignedUntil = assignedUntil,
            AssignedById = assignedById
        };

    // Maintenance helpers
    internal static CreateOrUpdateMaintenanceModel CreateMaintenanceModel(
        Guid itemId,
        Guid typeId,
        DateTimeOffset? performedAt = null,
        Guid? performedById = null,
        string? remarks = null)
        => new()
        {
            ItemId = itemId,
            TypeId = typeId,
            PerformedAt = performedAt ?? DateTimeOffset.UtcNow,
            PerformedById = performedById,
            Remarks = remarks
        };

    internal static Maintenance CreateMaintenance(
        Guid itemId,
        Guid typeId,
        Guid? id = null,
        DateTimeOffset? performedAt = null,
        Guid? performedById = null,
        string? remarks = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            ItemId = itemId,
            TypeId = typeId,
            PerformedAt = performedAt ?? DateTimeOffset.UtcNow,
            PerformedById = performedById,
            Remarks = remarks
        };

    // User helpers
    internal static User CreateUser(
        Guid? id = null,
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            EMail = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTimeOffset.UtcNow,
            LastSync = DateTimeOffset.UtcNow
        };

    // Order helpers
    internal static CreateOrUpdateOrderModel CreateOrderModel(
        DateTimeOffset? orderDate = null,
        OrderStatus status = OrderStatus.Draft,
        string? orderIdentifier = null,
        List<CreateOrUpdateOrderItemModel>? items = null)
        => new()
        {
            OrderDate = orderDate ?? DateTimeOffset.UtcNow,
            Status = status,
            OrderIdentifier = orderIdentifier,
            Items = items ?? []
        };

    internal static Order CreateOrder(
        Guid? id = null,
        DateTimeOffset? orderDate = null,
        OrderStatus status = OrderStatus.Draft,
        string? orderIdentifier = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            OrderDate = orderDate ?? DateTimeOffset.UtcNow,
            Status = status,
            OrderIdentifier = orderIdentifier
        };
}
