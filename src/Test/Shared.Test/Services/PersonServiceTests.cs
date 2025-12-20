using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for PersonService.
/// These tests focus on CRUD operations, duplicate name/external ID conflict detection, and department relationships.
/// </summary>
public class PersonServiceTests
{
    private readonly PersonMapper _mapper = new();

    [Fact]
    public async Task CreatePersonAsync_WithValidModel_ShouldCreatePerson()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var model = TestDataFactory.CreatePersonModel("Max", "Mustermann");

        // Act
        var result = await service.CreatePersonAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.FirstName, result.FirstName);
        Assert.Equal(model.LastName, result.LastName);
    }

    [Fact]
    public async Task CreatePersonAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var existingPerson = TestDataFactory.CreatePerson(firstName: "Max", lastName: "Mustermann");
        context.Persons.Add(existingPerson);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreatePersonModel("Max", "Mustermann");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreatePersonAsync(model));
    }

    [Fact]
    public async Task CreatePersonAsync_WithDuplicateExternalId_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var existingPerson = TestDataFactory.CreatePerson(firstName: "Max", lastName: "Mustermann", externalId: "EXT001");
        context.Persons.Add(existingPerson);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreatePersonModel("John", "Doe", externalId: "EXT001");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreatePersonAsync(model));
    }

    [Fact]
    public async Task GetAllPersonsAsync_ShouldReturnAllPersons_OrderedByLastNameThenFirstName()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        context.Persons.AddRange(
            TestDataFactory.CreatePerson(firstName: "Zoe", lastName: "Miller"),
            TestDataFactory.CreatePerson(firstName: "Alice", lastName: "Miller"),
            TestDataFactory.CreatePerson(firstName: "Bob", lastName: "Anderson")
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPersonsAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Bob", result[0].FirstName);
        Assert.Equal("Anderson", result[0].LastName);
        Assert.Equal("Alice", result[1].FirstName);
        Assert.Equal("Miller", result[1].LastName);
        Assert.Equal("Zoe", result[2].FirstName);
        Assert.Equal("Miller", result[2].LastName);
    }

    [Fact]
    public async Task GetAllPersonsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        // Act
        var result = await service.GetAllPersonsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPersonByIdAsync_WithExistingId_ShouldReturnPerson()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var person = TestDataFactory.CreatePerson(firstName: "John", lastName: "Doe");
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPersonByIdAsync(person.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(person.Id, result.Id);
        Assert.Equal(person.FirstName, result.FirstName);
        Assert.Equal(person.LastName, result.LastName);
    }

    [Fact]
    public async Task GetPersonByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        // Act
        var result = await service.GetPersonByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePersonAsync_WithExistingId_ShouldUpdatePerson()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var person = TestDataFactory.CreatePerson(firstName: "Original", lastName: "Name");
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreatePersonModel("Updated", "Person");

        // Act
        var result = await service.UpdatePersonAsync(person.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Persons.FindAsync(person.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.FirstName);
        Assert.Equal("Person", updated.LastName);
    }

    [Fact]
    public async Task UpdatePersonAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var updateModel = TestDataFactory.CreatePersonModel("New", "Person");

        // Act
        var result = await service.UpdatePersonAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdatePersonAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var existingPerson = TestDataFactory.CreatePerson(firstName: "Existing", lastName: "Name");
        var personToUpdate = TestDataFactory.CreatePerson(firstName: "Original", lastName: "Name");
        context.Persons.AddRange(existingPerson, personToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreatePersonModel("Existing", "Name");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdatePersonAsync(personToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task UpdatePersonAsync_WithDuplicateExternalId_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var existingPerson = TestDataFactory.CreatePerson(firstName: "Existing", lastName: "Person", externalId: "EXT001");
        var personToUpdate = TestDataFactory.CreatePerson(firstName: "Original", lastName: "Name", externalId: "EXT002");
        context.Persons.AddRange(existingPerson, personToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreatePersonModel("Updated", "Person", externalId: "EXT001");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdatePersonAsync(personToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeletePersonAsync_WithExistingId_ShouldDeletePerson()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var person = TestDataFactory.CreatePerson();
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeletePersonAsync(person.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Persons.FindAsync(person.Id));
    }

    [Fact]
    public async Task DeletePersonAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        // Act
        var result = await service.DeletePersonAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPersonsForDepartmentAsync_WithExistingDepartment_ShouldReturnPersons()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var department = TestDataFactory.CreateDepartment(name: "Fire Brigade A");
        var person1 = TestDataFactory.CreatePerson(firstName: "Zoe", lastName: "Anderson");
        person1.Departments.Add(department);
        var person2 = TestDataFactory.CreatePerson(firstName: "Alice", lastName: "Smith");
        person2.Departments.Add(department);
        var person3 = TestDataFactory.CreatePerson(firstName: "Bob", lastName: "Miller");
        // Person3 is not in the department

        context.Departments.Add(department);
        context.Persons.AddRange(person1, person2, person3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPersonsForDepartmentAsync(department.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Zoe", result[0].FirstName); // Anderson comes first alphabetically
        Assert.Equal("Alice", result[1].FirstName); // Smith comes second
    }

    [Fact]
    public async Task GetPersonsForDepartmentAsync_WithNonExistingDepartment_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetPersonsForDepartmentAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPersonsForDepartmentAsync_WithNoPersonsInDepartment_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new PersonService(context, _mapper);
        var department = TestDataFactory.CreateDepartment(name: "Empty Department");
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPersonsForDepartmentAsync(department.Id);

        // Assert
        Assert.Empty(result);
    }
}
