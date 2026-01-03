using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class DepartmentService(AppDbContext context, DepartmentMapper mapper) : IDepartmentService
{
    public async Task<DepartmentModel> CreateDepartmentAsync(CreateOrUpdateDepartmentModel model)
    {
        var exists = await context.Departments
            .AnyAsync(c => c.Name == model.Name);

        if (exists)
            throw new ConflictException("A department with the same name already exists.");

        var department = mapper.MapCreateOrUpdateDepartmentModelToDepartment(model);

        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        return mapper.MapDepartmentToDepartmentModel(department);
    }

    public async Task<PagedResult<DepartmentModel>> GetAllDepartmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Departments
            .OrderBy(d => d.Name)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectDepartmentsToDepartmentModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id)
    {
        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        return department is null ? null : mapper.MapDepartmentToDepartmentModel(department);
    }

    public async Task<bool> UpdateDepartmentAsync(Guid id, CreateOrUpdateDepartmentModel model)
    {
        var department = await context.Departments.FindAsync(id);
        if (department is null)
            return false;

        var nameExists = await context.Departments.AnyAsync(d =>
            d.Id != id && d.Name == model.Name);

        if (nameExists)
            throw new ConflictException("Another department with the same name already exists.");

        mapper.MapCreateOrUpdateDepartmentModelToDepartment(model, department);

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