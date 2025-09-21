using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class StorageLocationServiceTest
{
    private readonly StorageLocationMapper _mapper;

    public StorageLocationServiceTest()
    {
        _mapper = new StorageLocationMapper();
    }

    [Fact]
    public async Task CreateStorageLocationAsync_ShouldCreateLocation()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        var model = new CreateStorageLocationModel
        {
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };

        var result = await service.CreateStorageLocationAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Remarks, result.Remarks);

        var entity = await context.StorageLocations.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.Name, entity!.Name);
        Assert.Equal(model.Remarks, entity.Remarks);
    }

    [Fact]
    public async Task CreateStorageLocationAsync_ShouldThrowIfDuplicateName()
    {
        var context = TestHelper.GetTestDbContext();
        context.StorageLocations.Add(new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        });
        context.SaveChanges();

        var service = new StorageLocationService(context, _mapper);

        var model = new CreateStorageLocationModel
        {
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateStorageLocationAsync(model));
    }

    [Fact]
    public async Task GetAllStorageLocationsAsync_ShouldReturnAllLocations()
    {
        var context = TestHelper.GetTestDbContext();
        var location1 = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };
        var location2 = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Secondary Storage",
            Remarks = "Backup warehouse"
        };
        context.StorageLocations.Add(location1);
        context.StorageLocations.Add(location2);
        context.SaveChanges();

        var service = new StorageLocationService(context, _mapper);

        var result = await service.GetAllStorageLocationsAsync();

        Assert.Equal(2, result.Count);
        var main = result.FirstOrDefault(l => l.Name == "Main Storage");
        var secondary = result.FirstOrDefault(l => l.Name == "Secondary Storage");

        Assert.NotNull(main);
        Assert.Equal(location1.Id, main!.Id);
        Assert.Equal(location1.Name, main.Name);
        Assert.Equal(location1.Remarks, main.Remarks);

        Assert.NotNull(secondary);
        Assert.Equal(location2.Id, secondary!.Id);
        Assert.Equal(location2.Name, secondary.Name);
        Assert.Equal(location2.Remarks, secondary.Remarks);
    }

    [Fact]
    public async Task GetStorageLocationByIdAsync_ShouldReturnLocation()
    {
        var context = TestHelper.GetTestDbContext();
        var location = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };
        context.StorageLocations.Add(location);
        context.SaveChanges();

        var service = new StorageLocationService(context, _mapper);

        var result = await service.GetStorageLocationByIdAsync(location.Id);

        Assert.NotNull(result);
        Assert.Equal(location.Id, result!.Id);
        Assert.Equal(location.Name, result.Name);
        Assert.Equal(location.Remarks, result.Remarks);
    }

    [Fact]
    public async Task GetStorageLocationByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        var result = await service.GetStorageLocationByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStorageLocationAsync_ShouldUpdateLocation()
    {
        var context = TestHelper.GetTestDbContext();
        var location = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };
        context.StorageLocations.Add(location);
        context.SaveChanges();

        var service = new StorageLocationService(context, _mapper);

        var model = new StorageLocationModel
        {
            Id = location.Id,
            Name = "Main Storage Updated",
            Remarks = "Updated warehouse"
        };

        var result = await service.UpdateStorageLocationAsync(model);

        Assert.True(result);
        var updated = await context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated!.Name);
        Assert.Equal(model.Remarks, updated.Remarks);
    }

    [Fact]
    public async Task UpdateStorageLocationAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        var model = new StorageLocationModel
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };

        var result = await service.UpdateStorageLocationAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStorageLocationAsync_ShouldThrowIfDuplicateName()
    {
        var context = TestHelper.GetTestDbContext();
        var location1 = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };
        var location2 = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Secondary Storage",
            Remarks = "Backup warehouse"
        };
        context.StorageLocations.Add(location1);
        context.StorageLocations.Add(location2);
        context.SaveChanges();

        var service = new StorageLocationService(context, _mapper);

        var model = new StorageLocationModel
        {
            Id = location2.Id,
            Name = "Main Storage",
            Remarks = "Backup warehouse"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateStorageLocationAsync(model));
    }

    [Fact]
    public async Task DeleteStorageLocationAsync_ShouldDeleteLocation()
    {
        var context = TestHelper.GetTestDbContext();
        var location = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
            Remarks = "Central warehouse"
        };
        context.StorageLocations.Add(location);
        context.SaveChanges();

        var service = new StorageLocationService(context, _mapper);

        var result = await service.DeleteStorageLocationAsync(location.Id);

        Assert.True(result);
        Assert.False(context.StorageLocations.Any());
    }

    [Fact]
    public async Task DeleteStorageLocationAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        var result = await service.DeleteStorageLocationAsync(Guid.NewGuid());

        Assert.False(result);
    }
}