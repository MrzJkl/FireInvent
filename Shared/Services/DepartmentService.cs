using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class DepartmentService(GearDbContext context)
{
    public async Task<DepartmentModel> CreateDepartmentAsync(DepartmentModel model)
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description
        };

        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        return new DepartmentModel
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };
    }

    public async Task<List<DepartmentModel>> GetAllDepartmentsAsync()
    {
        return await context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentModel
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description
            })
            .ToListAsync();
    }

    public async Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id)
    {
        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department is null)
            return null;

        return new DepartmentModel
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };
    }

    public async Task<bool> UpdateDepartmentAsync(DepartmentModel model)
    {
        var department = await context.Departments.FindAsync(model.Id);
        if (department is null)
            return false;

        department.Name = model.Name;
        department.Description = model.Description;

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
