using AutoMapper;
using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class DepartmentServiceTest
{
    private readonly IMapper _mapper;

    public DepartmentServiceTest()
    {
        _mapper = TestHelper.GetMapper();
    }

    [Fact]
    public async Task CreateDepartmentAsync_ShouldCreateDepartment()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        var model = new CreateDepartmentModel
        {
            Name = "Fire Department",
            Description = "Handles fire emergencies"
        };

        var result = await service.CreateDepartmentAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);

        var entity = await context.Departments.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.Name, entity!.Name);
        Assert.Equal(model.Description, entity.Description);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ShouldReturnAllDepartments()
    {
        var context = TestHelper.GetTestDbContext();
        var department1 = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Fire Department",
            Description = "Handles fire emergencies"
        };
        var department2 = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Rescue Department",
            Description = "Handles rescue operations"
        };
        context.Departments.Add(department1);
        context.Departments.Add(department2);
        context.SaveChanges();

        var service = new DepartmentService(context, _mapper);

        var result = await service.GetAllDepartmentsAsync();

        Assert.Equal(2, result.Count);
        var fire = result.FirstOrDefault(d => d.Name == "Fire Department");
        var rescue = result.FirstOrDefault(d => d.Name == "Rescue Department");

        Assert.NotNull(fire);
        Assert.Equal(department1.Id, fire!.Id);
        Assert.Equal(department1.Name, fire.Name);
        Assert.Equal(department1.Description, fire.Description);

        Assert.NotNull(rescue);
        Assert.Equal(department2.Id, rescue!.Id);
        Assert.Equal(department2.Name, rescue.Name);
        Assert.Equal(department2.Description, rescue.Description);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ShouldReturnDepartment()
    {
        var context = TestHelper.GetTestDbContext();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Fire Department",
            Description = "Handles fire emergencies"
        };
        context.Departments.Add(department);
        context.SaveChanges();

        var service = new DepartmentService(context, _mapper);

        var result = await service.GetDepartmentByIdAsync(department.Id);

        Assert.NotNull(result);
        Assert.Equal(department.Id, result!.Id);
        Assert.Equal(department.Name, result.Name);
        Assert.Equal(department.Description, result.Description);
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        var result = await service.GetDepartmentByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_ShouldUpdateDepartment()
    {
        var context = TestHelper.GetTestDbContext();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Fire Department",
            Description = "Handles fire emergencies"
        };
        context.Departments.Add(department);
        context.SaveChanges();

        var service = new DepartmentService(context, _mapper);

        var model = new DepartmentModel
        {
            Id = department.Id,
            Name = "Fire Department Updated",
            Description = "Handles all emergencies"
        };

        var result = await service.UpdateDepartmentAsync(model);

        Assert.True(result);
        var updated = await context.Departments.FindAsync(department.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated!.Name);
        Assert.Equal(model.Description, updated.Description);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        var model = new DepartmentModel
        {
            Id = Guid.NewGuid(),
            Name = "Fire Department",
            Description = "Handles fire emergencies"
        };

        var result = await service.UpdateDepartmentAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_ShouldDeleteDepartment()
    {
        var context = TestHelper.GetTestDbContext();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = "Fire Department",
            Description = "Handles fire emergencies"
        };
        context.Departments.Add(department);
        context.SaveChanges();

        var service = new DepartmentService(context, _mapper);

        var result = await service.DeleteDepartmentAsync(department.Id);

        Assert.True(result);
        Assert.False(context.Departments.Any());
    }

    [Fact]
    public async Task DeleteDepartmentAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new DepartmentService(context, _mapper);

        var result = await service.DeleteDepartmentAsync(Guid.NewGuid());

        Assert.False(result);
    }
}