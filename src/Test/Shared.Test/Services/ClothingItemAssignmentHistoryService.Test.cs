using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class ClothingItemAssignmentHistoryServiceTest
{
    private readonly ItemAssignmentHistoryMapper _mapper = new();

    private static Item CreateItem(AppDbContext context)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = Contract.ProductType.Jacket
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
            Condition = Contract.ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1)
        };
        context.Items.Add(item);
        context.SaveChanges();
        return item;
    }

    private Person CreatePerson(AppDbContext context)
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
        };
        context.Persons.Add(person);
        context.SaveChanges();
        return person;
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldCreateAssignment()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new CreateItemAssignmentHistoryModel
        {
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 12, 31)
        };

        var result = await service.CreateAssignmentAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.ItemId, result.ItemId);
        Assert.Equal(model.PersonId, result.PersonId);
        Assert.Equal(model.AssignedFrom, result.AssignedFrom);
        Assert.Equal(model.AssignedUntil, result.AssignedUntil);

        var entity = await context.ItemAssignmentHistories.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.ItemId, entity!.ItemId);
        Assert.Equal(model.PersonId, entity.PersonId);
        Assert.Equal(model.AssignedFrom, entity.AssignedFrom);
        Assert.Equal(model.AssignedUntil, entity.AssignedUntil);
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldThrowIfItemNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var person = CreatePerson(context);
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new CreateItemAssignmentHistoryModel
        {
            ItemId = Guid.NewGuid(),
            PersonId = person.Id,
            AssignedFrom = DateTime.Now
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldThrowIfPersonNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new CreateItemAssignmentHistoryModel
        {
            ItemId = item.Id,
            PersonId = Guid.NewGuid(),
            AssignedFrom = DateTime.Now
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldThrowIfOverlappingAssignmentExists()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        context.ItemAssignmentHistories.Add(new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 12, 31)
        });
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new CreateItemAssignmentHistoryModel
        {
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 6, 1),
            AssignedUntil = new DateTime(2024, 12, 31)
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAssignmentAsync(model));
    }

    [Fact]
    public async Task GetAssignmentsForItemAsync_ShouldReturnAssignments()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment1 = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        var assignment2 = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 7, 1),
            AssignedUntil = null
        };
        context.ItemAssignmentHistories.Add(assignment1);
        context.ItemAssignmentHistories.Add(assignment2);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var result = await service.GetAssignmentsForItemAsync(item.Id);

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(a => a.AssignedFrom == assignment1.AssignedFrom);
        var second = result.FirstOrDefault(a => a.AssignedFrom == assignment2.AssignedFrom);

        Assert.NotNull(first);
        Assert.Equal(assignment1.Id, first!.Id);
        Assert.Equal(assignment1.ItemId, first.ItemId);
        Assert.Equal(assignment1.PersonId, first.PersonId);
        Assert.Equal(assignment1.AssignedFrom, first.AssignedFrom);
        Assert.Equal(assignment1.AssignedUntil, first.AssignedUntil);

        Assert.NotNull(second);
        Assert.Equal(assignment2.Id, second!.Id);
        Assert.Equal(assignment2.ItemId, second.ItemId);
        Assert.Equal(assignment2.PersonId, second.PersonId);
        Assert.Equal(assignment2.AssignedFrom, second.AssignedFrom);
        Assert.Equal(assignment2.AssignedUntil, second.AssignedUntil);
    }

    [Fact]
    public async Task GetAssignmentsForItemAsync_ShouldThrowIfItemNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAssignmentsForItemAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_ShouldReturnAllAssignments()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment1 = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        var assignment2 = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 7, 1),
            AssignedUntil = null
        };
        context.ItemAssignmentHistories.Add(assignment1);
        context.ItemAssignmentHistories.Add(assignment2);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var result = await service.GetAllAssignmentsAsync();

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(a => a.AssignedFrom == assignment1.AssignedFrom);
        var second = result.FirstOrDefault(a => a.AssignedFrom == assignment2.AssignedFrom);

        Assert.NotNull(first);
        Assert.Equal(assignment1.Id, first!.Id);
        Assert.Equal(assignment1.ItemId, first.ItemId);
        Assert.Equal(assignment1.PersonId, first.PersonId);
        Assert.Equal(assignment1.AssignedFrom, first.AssignedFrom);
        Assert.Equal(assignment1.AssignedUntil, first.AssignedUntil);

        Assert.NotNull(second);
        Assert.Equal(assignment2.Id, second!.Id);
        Assert.Equal(assignment2.ItemId, second.ItemId);
        Assert.Equal(assignment2.PersonId, second.PersonId);
        Assert.Equal(assignment2.AssignedFrom, second.AssignedFrom);
        Assert.Equal(assignment2.AssignedUntil, second.AssignedUntil);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_ShouldReturnAssignment()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        context.ItemAssignmentHistories.Add(assignment);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var result = await service.GetAssignmentByIdAsync(assignment.Id);

        Assert.NotNull(result);
        Assert.Equal(assignment.Id, result!.Id);
        Assert.Equal(assignment.ItemId, result.ItemId);
        Assert.Equal(assignment.PersonId, result.PersonId);
        Assert.Equal(assignment.AssignedFrom, result.AssignedFrom);
        Assert.Equal(assignment.AssignedUntil, result.AssignedUntil);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var result = await service.GetAssignmentByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldUpdateAssignment()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        context.ItemAssignmentHistories.Add(assignment);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new ItemAssignmentHistoryModel
        {
            Id = assignment.Id,
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 2, 1),
            AssignedUntil = new DateTime(2024, 7, 1)
        };

        var result = await service.UpdateAssignmentAsync(model);

        Assert.True(result);
        var updated = await context.ItemAssignmentHistories.FindAsync(assignment.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.ItemId, updated!.ItemId);
        Assert.Equal(model.PersonId, updated.PersonId);
        Assert.Equal(model.AssignedFrom, updated.AssignedFrom);
        Assert.Equal(model.AssignedUntil, updated.AssignedUntil);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new ItemAssignmentHistoryModel
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 2, 1),
            AssignedUntil = new DateTime(2024, 7, 1)
        };

        var result = await service.UpdateAssignmentAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldThrowIfItemNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        context.ItemAssignmentHistories.Add(assignment);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new ItemAssignmentHistoryModel
        {
            Id = assignment.Id,
            ItemId = Guid.NewGuid(), // not existing
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 2, 1),
            AssignedUntil = new DateTime(2024, 7, 1)
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAssignmentAsync(model));
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldThrowIfPersonNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        context.ItemAssignmentHistories.Add(assignment);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new ItemAssignmentHistoryModel
        {
            Id = assignment.Id,
            ItemId = item.Id,
            PersonId = Guid.NewGuid(), // not existing
            AssignedFrom = new DateTime(2024, 2, 1),
            AssignedUntil = new DateTime(2024, 7, 1)
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAssignmentAsync(model));
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldThrowIfOverlappingAssignmentExists()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment1 = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        var assignment2 = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 7, 1),
            AssignedUntil = null
        };
        context.ItemAssignmentHistories.Add(assignment1);
        context.ItemAssignmentHistories.Add(assignment2);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var model = new ItemAssignmentHistoryModel
        {
            Id = assignment2.Id,
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 5, 1),
            AssignedUntil = new DateTime(2024, 8, 1)
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAssignmentAsync(model));
    }

    [Fact]
    public async Task DeleteAssignmentAsync_ShouldDeleteAssignment()
    {
        var context = TestHelper.GetTestDbContext();
        var item = CreateItem(context);
        var person = CreatePerson(context);
        var assignment = new ItemAssignmentHistory
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            PersonId = person.Id,
            AssignedFrom = new DateTime(2024, 1, 1),
            AssignedUntil = new DateTime(2024, 6, 1)
        };
        context.ItemAssignmentHistories.Add(assignment);
        context.SaveChanges();

        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var result = await service.DeleteAssignmentAsync(assignment.Id);

        Assert.True(result);
        Assert.False(context.ItemAssignmentHistories.Any());
    }

    [Fact]
    public async Task DeleteAssignmentAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemAssignmentHistoryService(context, _mapper);

        var result = await service.DeleteAssignmentAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
