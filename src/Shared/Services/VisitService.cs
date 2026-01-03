using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VisitService(AppDbContext context, VisitMapper mapper) : IVisitService
{
    public async Task<VisitModel?> GetVisitByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var visit = await context.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        return visit is null ? null : mapper.MapVisitToVisitModel(visit);
    }

    public async Task<PagedResult<VisitModel>> GetAllVisitsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Visits
            .OrderBy(v => v.CreatedAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectVisitsToVisitModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<VisitModel> CreateVisitAsync(CreateOrUpdateVisitModel model, CancellationToken cancellationToken = default)
    {
        // Validate that Appointment exists
        _ = await context.Appointments.FindAsync(model.AppointmentId, cancellationToken) 
            ?? throw new BadRequestException($"Appointment with ID '{model.AppointmentId}' does not exist.");

        // Validate that Person exists
        _ = await context.Persons.FindAsync(model.PersonId, cancellationToken) 
            ?? throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        // Check for unique constraint: AppointmentId + PersonId
        var visitExists = await context.Visits.AnyAsync(v =>
            v.AppointmentId == model.AppointmentId && v.PersonId == model.PersonId, cancellationToken);

        if (visitExists)
            throw new ConflictException($"A visit for person '{model.PersonId}' at appointment '{model.AppointmentId}' already exists.");

        var visit = mapper.MapCreateOrUpdateVisitModelToVisit(model);

        await context.Visits.AddAsync(visit, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        visit = await context.Visits
            .AsNoTracking()
            .FirstAsync(v => v.Id == visit.Id, cancellationToken);

        return mapper.MapVisitToVisitModel(visit);
    }

    public async Task<bool> UpdateVisitAsync(Guid id, CreateOrUpdateVisitModel model, CancellationToken cancellationToken = default)
    {
        // Validate that Appointment exists
        _ = await context.Appointments.FindAsync(model.AppointmentId, cancellationToken) 
            ?? throw new BadRequestException($"Appointment with ID '{model.AppointmentId}' does not exist.");

        // Validate that Person exists
        _ = await context.Persons.FindAsync(model.PersonId, cancellationToken) 
            ?? throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        var entity = await context.Visits
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (entity is null)
            return false;

        // Check for unique constraint: AppointmentId + PersonId (excluding current visit)
        var visitDuplicate = await context.Visits.AnyAsync(v =>
            v.Id != id &&
            v.AppointmentId == model.AppointmentId && 
            v.PersonId == model.PersonId, cancellationToken);

        if (visitDuplicate)
            throw new ConflictException($"A visit for person '{model.PersonId}' at appointment '{model.AppointmentId}' already exists.");

        mapper.MapCreateOrUpdateVisitModelToVisit(model, entity);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteVisitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Visits
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (entity is null)
            return false;

        context.Visits.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<VisitModel>> GetVisitsForAppointmentAsync(Guid appointmentId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var appointmentExists = await context.Appointments.AnyAsync(a => a.Id == appointmentId);
        if (!appointmentExists)
            throw new NotFoundException($"Appointment with ID {appointmentId} not found.");

        var query = context.Visits
            .Where(v => v.AppointmentId == appointmentId)
            .OrderBy(v => v.CreatedAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectVisitsToVisitModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }
}
