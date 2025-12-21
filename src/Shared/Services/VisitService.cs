using FireInvent.Database;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VisitService(AppDbContext context, VisitMapper mapper) : IVisitService
{
    public async Task<VisitModel?> GetVisitByIdAsync(Guid id)
    {
        var visit = await context.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return visit is null ? null : mapper.MapVisitToVisitModel(visit);
    }

    public async Task<List<VisitModel>> GetAllVisitsAsync()
    {
        var visits = await context.Visits
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapVisitsToVisitModels(visits);
    }

    public async Task<VisitModel> CreateVisitAsync(CreateOrUpdateVisitModel model)
    {
        // Validate that Appointment exists
        _ = await context.Appointments.FindAsync(model.AppointmentId) 
            ?? throw new BadRequestException($"Appointment with ID '{model.AppointmentId}' does not exist.");

        // Validate that Person exists
        _ = await context.Persons.FindAsync(model.PersonId) 
            ?? throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        // Check for unique constraint: AppointmentId + PersonId
        var visitExists = await context.Visits.AnyAsync(v =>
            v.AppointmentId == model.AppointmentId && v.PersonId == model.PersonId);

        if (visitExists)
            throw new ConflictException($"A visit for person '{model.PersonId}' at appointment '{model.AppointmentId}' already exists.");

        var visit = mapper.MapCreateOrUpdateVisitModelToVisit(model);

        await context.Visits.AddAsync(visit);
        await context.SaveChangesAsync();

        visit = await context.Visits
            .AsNoTracking()
            .FirstAsync(v => v.Id == visit.Id);

        return mapper.MapVisitToVisitModel(visit);
    }

    public async Task<bool> UpdateVisitAsync(Guid id, CreateOrUpdateVisitModel model)
    {
        // Validate that Appointment exists
        _ = await context.Appointments.FindAsync(model.AppointmentId) 
            ?? throw new BadRequestException($"Appointment with ID '{model.AppointmentId}' does not exist.");

        // Validate that Person exists
        _ = await context.Persons.FindAsync(model.PersonId) 
            ?? throw new BadRequestException($"Person with ID '{model.PersonId}' does not exist.");

        var entity = await context.Visits
            .FirstOrDefaultAsync(v => v.Id == id);

        if (entity is null)
            return false;

        // Check for unique constraint: AppointmentId + PersonId (excluding current visit)
        var visitDuplicate = await context.Visits.AnyAsync(v =>
            v.Id != id &&
            v.AppointmentId == model.AppointmentId && 
            v.PersonId == model.PersonId);

        if (visitDuplicate)
            throw new ConflictException($"A visit for person '{model.PersonId}' at appointment '{model.AppointmentId}' already exists.");

        mapper.MapCreateOrUpdateVisitModelToVisit(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVisitAsync(Guid id)
    {
        var entity = await context.Visits
            .FirstOrDefaultAsync(v => v.Id == id);

        if (entity is null)
            return false;

        context.Visits.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<VisitModel>> GetVisitsForAppointmentAsync(Guid appointmentId)
    {
        var appointmentExists = await context.Appointments.AnyAsync(a => a.Id == appointmentId);
        if (!appointmentExists)
            throw new NotFoundException($"Appointment with ID {appointmentId} not found.");

        var visits = await context.Visits
            .Where(v => v.AppointmentId == appointmentId)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapVisitsToVisitModels(visits);
    }
}
