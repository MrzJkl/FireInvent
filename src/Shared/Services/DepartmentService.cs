using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class DepartmentService(AppDbContext context, DepartmentMapper mapper) : IDepartmentService
{
    public async Task<DepartmentModel> CreateDepartmentAsync(CreateDepartmentModel model)
    {
        var exists = await context.Departments
            .AnyAsync(c => c.Name == model.Name);

        if (exists)
            throw new ConflictException("A aepartment with the same name already exists.");

        var department = mapper.MapCreateDepartmentModelToDepartment(model);

        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        return mapper.MapDepartmentToDepartmentModel(department);
    }

    public async Task<List<DepartmentModel>> GetAllDepartmentsAsync()
    {
        var departments = await context.Departments
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();

        return mapper.MapDepartmentsToDepartmentModels(departments);
    }

    public async Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id)
    {
        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return department is null ? null : mapper.MapDepartmentToDepartmentModel(department);
    }

    public async Task<bool> UpdateDepartmentAsync(DepartmentModel model)
    {
        var department = await context.Departments.FindAsync(model.Id);
        if (department is null)
            return false;

        var nameExists = await context.Departments.AnyAsync(d =>
            d.Id != model.Id && d.Name == model.Name);

        if (nameExists)
            throw new ConflictException("Another department with the same name already exists.");

        mapper.MapDepartmentModelToDepartment(model, department);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDepartmentAsync(Guid id)
    {
        var department = await context.Departments.FindAsync(id);
        if (department is null)
            return false;

        context.Departments.Remove(department);
        await context.SaveChangesAsync();
        return true;
    }
}