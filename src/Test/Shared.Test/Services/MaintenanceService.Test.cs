using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Moq;

namespace FireInvent.Test.Shared.Services;

public class MaintenanceServiceTest
{
    private readonly IMapper _mapper;

    public MaintenanceServiceTest()
    {
        _mapper = TestHelper.GetMapper();
    }

    private static Item CreateItem(AppDbContext context)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = ProductType.Jacket
        };
        context.Products.Add(product);
        var variant = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        context.Variants.Add(variant);
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1)
        };
        context.Items.Add(item);
        context.SaveChanges();
        return item;
    }

    private static UserModel CreateUserModel()
    {
        return new UserModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Admin",
            LastName = "User",
            EMail = "admin@domain.com"
        };
    }

    [Fact]
    public async Task CreateMaintenanceAsync_ShouldCreateMaintenance()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var user = CreateUserModel();

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new CreateMaintenanceModel
        {
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
            PerformedById = user.Id,
            PerformedBy = user
        };

        var result = await service.CreateMaintenanceAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.ItemId, result.ItemId);
        Assert.Equal(model.PerformedAt, result.PerformedAt);
        Assert.Equal(model.MaintenanceType, result.MaintenanceType);
        Assert.Equal(model.Remarks, result.Remarks);
        Assert.Equal(model.PerformedById, result.PerformedById);

        var entity = await context.Maintenances.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.ItemId, entity!.ItemId);
        Assert.Equal(model.PerformedAt, entity.PerformedAt);
        Assert.Equal(model.MaintenanceType, entity.MaintenanceType);
        Assert.Equal(model.Remarks, entity.Remarks);
        Assert.Equal(model.PerformedById, entity.PeformedById);
    }

    [Fact]
    public async Task CreateMaintenanceAsync_ShouldThrowIfItemNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var user = CreateUserModel();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new CreateMaintenanceModel
        {
            ItemId = Guid.NewGuid(),
            PerformedAt = DateTime.Now,
            MaintenanceType = MaintenanceType.Washing
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateMaintenanceAsync(model));
    }

    [Fact]
    public async Task CreateMaintenanceAsync_ShouldThrowIfUserNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserModel?)null);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new CreateMaintenanceModel
        {
            ItemId = item.Id,
            PerformedAt = DateTime.Now,
            MaintenanceType = MaintenanceType.Washing,
            PerformedById = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateMaintenanceAsync(model));
    }

    [Fact]
    public async Task GetAllMaintenancesAsync_ShouldReturnAllMaintenances()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var maintenance1 = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
        };
        var maintenance2 = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 3, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Repaired zipper",
        };
        context.Maintenances.Add(maintenance1);
        context.Maintenances.Add(maintenance2);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var result = await service.GetAllMaintenancesAsync();

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(m => m.Id == maintenance1.Id);
        var second = result.FirstOrDefault(m => m.Id == maintenance2.Id);

        Assert.NotNull(first);
        Assert.Equal(maintenance1.ItemId, first!.ItemId);
        Assert.Equal(maintenance1.PerformedAt, first.PerformedAt);
        Assert.Equal(maintenance1.MaintenanceType, first.MaintenanceType);
        Assert.Equal(maintenance1.Remarks, first.Remarks);

        Assert.NotNull(second);
        Assert.Equal(maintenance2.ItemId, second!.ItemId);
        Assert.Equal(maintenance2.PerformedAt, second.PerformedAt);
        Assert.Equal(maintenance2.MaintenanceType, second.MaintenanceType);
        Assert.Equal(maintenance2.Remarks, second.Remarks);
    }

    [Fact]
    public async Task GetMaintenanceByIdAsync_ShouldReturnMaintenance()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
        };
        context.Maintenances.Add(maintenance);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var result = await service.GetMaintenanceByIdAsync(maintenance.Id);

        Assert.NotNull(result);
        Assert.Equal(maintenance.Id, result!.Id);
        Assert.Equal(maintenance.ItemId, result.ItemId);
        Assert.Equal(maintenance.PerformedAt, result.PerformedAt);
        Assert.Equal(maintenance.MaintenanceType, result.MaintenanceType);
        Assert.Equal(maintenance.Remarks, result.Remarks);
    }

    [Fact]
    public async Task GetMaintenanceByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var result = await service.GetMaintenanceByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldUpdateMaintenance()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var user = CreateUserModel();
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
        };
        context.Maintenances.Add(maintenance);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new MaintenanceModel
        {
            Id = maintenance.Id,
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 3, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Repaired zipper",
        };

        var result = await service.UpdateMaintenanceAsync(model);

        Assert.True(result);
        var updated = await context.Maintenances.FindAsync(maintenance.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.ItemId, updated!.ItemId);
        Assert.Equal(model.PerformedAt, updated.PerformedAt);
        Assert.Equal(model.MaintenanceType, updated.MaintenanceType);
        Assert.Equal(model.Remarks, updated.Remarks);
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var user = CreateUserModel();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new MaintenanceModel
        {
            Id = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            PerformedAt = DateTime.Now,
            MaintenanceType = MaintenanceType.Washing
        };

        var result = await service.UpdateMaintenanceAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldThrowIfItemNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var user = CreateUserModel();
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
            PeformedById = user.Id
        };
        context.Maintenances.Add(maintenance);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new MaintenanceModel
        {
            Id = maintenance.Id,
            ItemId = Guid.NewGuid(), // not existing
            PerformedAt = DateTime.Now,
            MaintenanceType = MaintenanceType.Washing
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateMaintenanceAsync(model));
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_ShouldThrowIfUserNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var user = CreateUserModel();
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
            PeformedById = user.Id
        };
        context.Maintenances.Add(maintenance);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserModel?)null);

        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var model = new MaintenanceModel
        {
            Id = maintenance.Id,
            ItemId = item.Id,
            PerformedAt = DateTime.Now,
            MaintenanceType = MaintenanceType.Washing,
            PerformedById = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateMaintenanceAsync(model));
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_ShouldDeleteMaintenance()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var maintenance = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
            PeformedById = Guid.NewGuid()
        };
        context.Maintenances.Add(maintenance);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var result = await service.DeleteMaintenanceAsync(maintenance.Id);

        Assert.True(result);
        Assert.False(context.Maintenances.Any());
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var result = await service.DeleteMaintenanceAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task GetMaintenancesForItemAsync_ShouldReturnMaintenancesForItem()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var otherItem = CreateItem(context);
        var maintenance1 = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 2, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned and checked",
            PeformedById = Guid.NewGuid()
        };
        var maintenance2 = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PerformedAt = new DateTime(2024, 3, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Repaired zipper",
            PeformedById = null
        };
        var maintenanceOther = new Maintenance
        {
            Id = Guid.NewGuid(),
            ItemId = otherItem.Id,
            PerformedAt = new DateTime(2024, 4, 1),
            MaintenanceType = MaintenanceType.Washing,
            Remarks = "Cleaned other item",
            PeformedById = null
        };
        context.Maintenances.Add(maintenance1);
        context.Maintenances.Add(maintenance2);
        context.Maintenances.Add(maintenanceOther);
        context.SaveChanges();

        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        var result = await service.GetMaintenancesForItemAsync(item.Id);

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(m => m.Id == maintenance1.Id);
        var second = result.FirstOrDefault(m => m.Id == maintenance2.Id);

        Assert.NotNull(first);
        Assert.Equal(maintenance1.ItemId, first!.ItemId);
        Assert.Equal(maintenance1.PerformedAt, first.PerformedAt);
        Assert.Equal(maintenance1.MaintenanceType, first.MaintenanceType);
        Assert.Equal(maintenance1.Remarks, first.Remarks);

        Assert.NotNull(second);
        Assert.Equal(maintenance2.ItemId, second!.ItemId);
        Assert.Equal(maintenance2.PerformedAt, second.PerformedAt);
        Assert.Equal(maintenance2.MaintenanceType, second.MaintenanceType);
        Assert.Equal(maintenance2.Remarks, second.Remarks);
    }

    [Fact]
    public async Task GetMaintenancesForItemAsync_ShouldThrowIfItemNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var userServiceMock = new Mock<IUserService>();
        var service = new MaintenanceService(context, _mapper, userServiceMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetMaintenancesForItemAsync(Guid.NewGuid()));
    }
}