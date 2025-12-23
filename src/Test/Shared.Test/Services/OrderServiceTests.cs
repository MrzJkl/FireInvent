using FireInvent.Contract;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for OrderService.
/// These tests focus on business logic (status restrictions) and data persistence.
/// </summary>
public class OrderServiceTests
{
    private readonly OrderMapper _mapper = new();

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);

        // Act
        var result = await service.GetOrderByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);

        // Act
        var result = await service.GetAllOrdersAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidModel_ShouldCreateOrder()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);
        var model = TestDataFactory.CreateOrderModel(
            orderDate: DateOnly.FromDateTime(DateTime.UtcNow),
            status: OrderStatus.Draft,
            orderIdentifier: "ORD-001");

        // Act
        var result = await service.CreateOrderAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        var savedOrder = await context.Orders.FindAsync(result.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal("ORD-001", savedOrder.OrderIdentifier);
        Assert.Equal(OrderStatus.Draft, savedOrder.Status);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);
        var updateModel = TestDataFactory.CreateOrderModel();

        // Act
        var result = await service.UpdateOrderAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithCompletedOrder_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);
        var order = TestDataFactory.CreateOrder(status: OrderStatus.Completed);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateOrderModel(status: OrderStatus.Draft);

        // Act
        var result = await service.UpdateOrderAsync(order.Id, updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_WithValidModel_ShouldUpdateOrder()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);
        var order = TestDataFactory.CreateOrder(status: OrderStatus.Draft, orderIdentifier: "ORD-001");
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateOrderModel(
            status: OrderStatus.Submitted,
            orderIdentifier: "ORD-001-UPDATED");

        // Act
        var result = await service.UpdateOrderAsync(order.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Submitted, updated.Status);
        Assert.Equal("ORD-001-UPDATED", updated.OrderIdentifier);
    }

    [Fact]
    public async Task DeleteOrderAsync_WithExistingId_ShouldDeleteOrder()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);
        var order = TestDataFactory.CreateOrder();
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteOrderAsync(order.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Orders.FindAsync(order.Id));
    }

    [Fact]
    public async Task DeleteOrderAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new OrderService(context, _mapper);

        // Act
        var result = await service.DeleteOrderAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
