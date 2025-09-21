using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class PersonServiceTest
{
    private readonly PersonMapper _mapper;

    public PersonServiceTest()
    {
        _mapper = new PersonMapper();
    }

    [Fact]
    public async Task CreatePersonAsync_ShouldCreatePerson()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        var model = new CreatePersonModel
        {
            FirstName = "Max",
            LastName = "Mustermann",
            Remarks = "Test remarks",
            ContactInfo = "max@mustermann.de",
            ExternalId = "EXT123"
        };

        var result = await service.CreatePersonAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.FirstName, result.FirstName);
        Assert.Equal(model.LastName, result.LastName);
        Assert.Equal(model.Remarks, result.Remarks);
        Assert.Equal(model.ContactInfo, result.ContactInfo);
        Assert.Equal(model.ExternalId, result.ExternalId);

        var entity = await context.Persons.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.FirstName, entity!.FirstName);
        Assert.Equal(model.LastName, entity.LastName);
        Assert.Equal(model.Remarks, entity.Remarks);
        Assert.Equal(model.ContactInfo, entity.ContactInfo);
        Assert.Equal(model.ExternalId, entity.ExternalId);
    }

    [Fact]
    public async Task CreatePersonAsync_ShouldThrowIfDuplicateName()
    {
        var context = TestHelper.GetTestDbContext();
        context.Persons.Add(new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann"
        });
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var model = new CreatePersonModel
        {
            FirstName = "Max",
            LastName = "Mustermann"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreatePersonAsync(model));
    }

    [Fact]
    public async Task CreatePersonAsync_ShouldThrowIfDuplicateExternalId()
    {
        var context = TestHelper.GetTestDbContext();
        context.Persons.Add(new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            ExternalId = "EXT123"
        });
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var model = new CreatePersonModel
        {
            FirstName = "Max",
            LastName = "Mustermann",
            ExternalId = "EXT123"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreatePersonAsync(model));
    }

    [Fact]
    public async Task GetAllPersonsAsync_ShouldReturnAllPersons()
    {
        var context = TestHelper.GetTestDbContext();
        var person1 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            Remarks = "Test remarks",
            ContactInfo = "max@mustermann.de",
            ExternalId = "EXT123"
        };
        var person2 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Remarks = "Other remarks",
            ContactInfo = "john@doe.com",
            ExternalId = "EXT456"
        };
        context.Persons.Add(person1);
        context.Persons.Add(person2);
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var result = await service.GetAllPersonsAsync();

        Assert.Equal(2, result.Count);
        var max = result.FirstOrDefault(p => p.FirstName == "Max");
        var john = result.FirstOrDefault(p => p.FirstName == "John");

        Assert.NotNull(max);
        Assert.Equal(person1.Id, max!.Id);
        Assert.Equal(person1.LastName, max.LastName);
        Assert.Equal(person1.Remarks, max.Remarks);
        Assert.Equal(person1.ContactInfo, max.ContactInfo);
        Assert.Equal(person1.ExternalId, max.ExternalId);

        Assert.NotNull(john);
        Assert.Equal(person2.Id, john!.Id);
        Assert.Equal(person2.LastName, john.LastName);
        Assert.Equal(person2.Remarks, john.Remarks);
        Assert.Equal(person2.ContactInfo, john.ContactInfo);
        Assert.Equal(person2.ExternalId, john.ExternalId);
    }

    [Fact]
    public async Task GetPersonByIdAsync_ShouldReturnPerson()
    {
        var context = TestHelper.GetTestDbContext();
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            Remarks = "Test remarks",
            ContactInfo = "max@mustermann.de",
            ExternalId = "EXT123"
        };
        context.Persons.Add(person);
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var result = await service.GetPersonByIdAsync(person.Id);

        Assert.NotNull(result);
        Assert.Equal(person.Id, result!.Id);
        Assert.Equal(person.FirstName, result.FirstName);
        Assert.Equal(person.LastName, result.LastName);
        Assert.Equal(person.Remarks, result.Remarks);
        Assert.Equal(person.ContactInfo, result.ContactInfo);
        Assert.Equal(person.ExternalId, result.ExternalId);
    }

    [Fact]
    public async Task GetPersonByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        var result = await service.GetPersonByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldUpdatePerson()
    {
        var context = TestHelper.GetTestDbContext();
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            Remarks = "Test remarks",
            ContactInfo = "max@mustermann.de",
            ExternalId = "EXT123"
        };
        context.Persons.Add(person);
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var model = new PersonModel
        {
            Id = person.Id,
            FirstName = "Maximilian",
            LastName = "Mustermann",
            Remarks = "Updated remarks",
            ContactInfo = "max@mustermann.de",
            ExternalId = "EXT999"
        };

        var result = await service.UpdatePersonAsync(model);

        Assert.True(result);
        var updated = await context.Persons.FindAsync(person.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.FirstName, updated!.FirstName);
        Assert.Equal(model.LastName, updated.LastName);
        Assert.Equal(model.Remarks, updated.Remarks);
        Assert.Equal(model.ContactInfo, updated.ContactInfo);
        Assert.Equal(model.ExternalId, updated.ExternalId);
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        var model = new PersonModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            Remarks = "Test remarks",
            ContactInfo = "max@mustermann.de",
            ExternalId = "EXT123"
        };

        var result = await service.UpdatePersonAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldThrowIfDuplicateName()
    {
        var context = TestHelper.GetTestDbContext();
        var person1 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann"
        };
        var person2 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };
        context.Persons.Add(person1);
        context.Persons.Add(person2);
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var model = new PersonModel
        {
            Id = person2.Id,
            FirstName = "Max",
            LastName = "Mustermann"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdatePersonAsync(model));
    }

    [Fact]
    public async Task UpdatePersonAsync_ShouldThrowIfDuplicateExternalId()
    {
        var context = TestHelper.GetTestDbContext();
        var person1 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann",
            ExternalId = "EXT123"
        };
        var person2 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            ExternalId = "EXT999"
        };
        context.Persons.Add(person1);
        context.Persons.Add(person2);
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var model = new PersonModel
        {
            Id = person2.Id,
            FirstName = "John",
            LastName = "Doe",
            ExternalId = "EXT123"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdatePersonAsync(model));
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldDeletePerson()
    {
        var context = TestHelper.GetTestDbContext();
        var person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Max",
            LastName = "Mustermann"
        };
        context.Persons.Add(person);
        context.SaveChanges();

        var service = new PersonService(context, _mapper);

        var result = await service.DeletePersonAsync(person.Id);

        Assert.True(result);
        Assert.False(context.Persons.Any());
    }

    [Fact]
    public async Task DeletePersonAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        var result = await service.DeletePersonAsync(Guid.NewGuid());

        Assert.False(result);
    }
}