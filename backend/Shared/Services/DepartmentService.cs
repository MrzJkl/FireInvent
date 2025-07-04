using AutoMapper;
using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class DepartmentService(GearDbContext context, IMapper mapper)
{
    public async Task<DepartmentModel> CreateDepartmentAsync(CreateDepartmentModel model)
    {
        var department = mapper.Map<Department>(model);
        department.Id = Guid.NewGuid();

        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        return mapper.Map<DepartmentModel>(department);
    }

    public async Task<List<DepartmentModel>> GetAllDepartmentsAsync()
    {
        var departments = await context.Departments
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<DepartmentModel>>(departments);
    }

    public async Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id)
    {
        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return department is null ? null : mapper.Map<DepartmentModel>(department);
    }

    public async Task<bool> UpdateDepartmentAsync(DepartmentModel model)
    {
        var department = await context.Departments.FindAsync(model.Id);
        if (department is null)
            return false;

        mapper.Map(model, department);

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