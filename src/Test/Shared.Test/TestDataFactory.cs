using FireInvent.Contract;
using FireInvent.Database.Models;
using FireInvent.Shared.Models;

namespace FireInvent.Test.Shared;

/// <summary>
/// Factory class for creating test data instances.
/// </summary>
internal static partial class TestDataFactory
{
    // Default test user ID for assignment operations
    internal static readonly Guid DefaultTestUserId = Guid.Parse("00000000-0000-0000-0000-000000000100");

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

    // Manufacturer helpers
    internal static CreateOrUpdateManufacturerModel CreateManufacturerModel(
        string name = "Test Manufacturer",
        string? description = null)
        => new()
        {
            Name = name,
            Description = description
        };

    internal static Manufacturer CreateManufacturer(
        Guid? id = null,
        string name = "Test Manufacturer",
        string? description = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description
        };

    // Product helpers
    internal static CreateOrUpdateProductModel CreateProductModel(
        Guid typeId,
        Guid manufacturerId,
        string name = "Test Product",
        string? description = null,
        string? externalIdentifier = null)
        => new()
        {
            TypeId = typeId,
            Name = name,
            ManufacturerId = manufacturerId,
            Description = description,
            ExternalIdentifier = externalIdentifier
        };

    internal static Product CreateProduct(
        Guid typeId,
        Guid manufacturerId,
        Guid? id = null,
        string name = "Test Product",
        string? description = null,
        string? externalIdentifier = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            TypeId = typeId,
            Name = name,
            ManufacturerId = manufacturerId,
            Description = description,
            ExternalIdentifier = externalIdentifier
        };

    // Variant helpers
    internal static CreateOrUpdateVariantModel CreateVariantModel(
        Guid productId,
        string name = "Test Variant",
        string? additionalSpecs = null,
        string? externalIdentifier = null)
        => new()
        {
            ProductId = productId,
            Name = name,
            AdditionalSpecs = additionalSpecs,
            ExternalIdentifier = externalIdentifier
        };

    internal static Variant CreateVariant(
        Guid productId,
        Guid? id = null,
        string name = "Test Variant",
        string? additionalSpecs = null,
        string? externalIdentifier = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            ProductId = productId,
            Name = name,
            AdditionalSpecs = additionalSpecs,
            ExternalIdentifier = externalIdentifier
        };

    // Item helpers
    internal static CreateOrUpdateItemModel CreateItemModel(
        Guid variantId,
        ItemCondition condition = ItemCondition.New,
        DateOnly? purchaseDate = null,
        string? identifier = null,
        Guid? storageLocationId = null)
        => new()
        {
            VariantId = variantId,
            Condition = condition,
            PurchaseDate = purchaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Identifier = identifier,
            StorageLocationId = storageLocationId
        };

    internal static Item CreateItem(
        Guid variantId,
        Guid? id = null,
        ItemCondition condition = ItemCondition.New,
        DateOnly? purchaseDate = null,
        string? identifier = null,
        Guid? storageLocationId = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            VariantId = variantId,
            Condition = condition,
            PurchaseDate = purchaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Identifier = identifier,
            StorageLocationId = storageLocationId
        };

    // ItemAssignmentHistory helpers
    internal static CreateOrUpdateItemAssignmentHistoryModel CreateAssignmentModel(
        Guid itemId,
        Guid personId,
        DateOnly? assignedFrom = null,
        DateOnly? assignedUntil = null,
        Guid? assignedById = null)
        => new()
        {
            ItemId = itemId,
            PersonId = personId,
            AssignedFrom = assignedFrom ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AssignedUntil = assignedUntil,
            AssignedById = assignedById ?? DefaultTestUserId
        };

    internal static ItemAssignmentHistory CreateAssignment(
        Guid itemId,
        Guid personId,
        Guid? id = null,
        DateOnly? assignedFrom = null,
        DateOnly? assignedUntil = null,
        Guid? assignedById = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            ItemId = itemId,
            PersonId = personId,
            AssignedFrom = assignedFrom ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AssignedUntil = assignedUntil,
            AssignedById = assignedById ?? DefaultTestUserId
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
            PerformedById = performedById.HasValue ? performedById.Value : DefaultTestUserId,
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
            PerformedById = performedById.HasValue ? performedById.Value : DefaultTestUserId,
            Remarks = remarks
        };

    // Order helpers
    internal static CreateOrUpdateOrderModel CreateOrderModel(
        DateOnly? orderDate = null,
        OrderStatus status = OrderStatus.Draft,
        string? orderIdentifier = null)
        => new()
        {
            OrderDate = orderDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = status,
            OrderIdentifier = orderIdentifier
        };
    internal static Order CreateOrder(
        Guid? id = null,
        DateOnly? orderDate = null,
        OrderStatus status = OrderStatus.Draft,
        string? orderIdentifier = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            OrderDate = orderDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = status,
            OrderIdentifier = orderIdentifier
        };

    // OrderItem helpers
    internal static CreateOrUpdateOrderItemModel CreateOrderItemModel(
        Guid orderId,
        Guid variantId,
        int quantity = 1,
        Guid? personId = null)
        => new()
        {
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
            PersonId = personId
        };

    internal static OrderItem CreateOrderItem(
        Guid orderId,
        Guid variantId,
        Guid? id = null,
        int quantity = 1,
        Guid? personId = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
            PersonId = personId
        };
}

