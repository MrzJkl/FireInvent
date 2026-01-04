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
    public async Task<DepartmentModel> CreateDepartmentAsync(CreateOrUpdateDepartmentModel model, CancellationToken cancellationToken = default)
    {
        var exists = await context.Departments
            .AnyAsync(c => c.Name == model.Name, cancellationToken);

        if (exists)
            throw new ConflictException("A department with the same name already exists.");

        var department = mapper.MapCreateOrUpdateDepartmentModelToDepartment(model);

        await context.Departments.AddAsync(department, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

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

    public async Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return department is null ? null : mapper.MapDepartmentToDepartmentModel(department);
    }

    public async Task<bool> UpdateDepartmentAsync(Guid id, CreateOrUpdateDepartmentModel model, CancellationToken cancellationToken = default)
    {
        var department = await context.Departments.FindAsync(id, cancellationToken);
        if (department is null)
            return false;

        var nameExists = await context.Departments.AnyAsync(d =>
            d.Id != id && d.Name == model.Name, cancellationToken);

        if (nameExists)
            throw new ConflictException("Another department with the same name already exists.");

        mapper.MapCreateOrUpdateDepartmentModelToDepartment(model, department);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await context.Departments.FindAsync(id, cancellationToken);
        if (department is null)
            return false;

        context.Departments.Remove(department);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}