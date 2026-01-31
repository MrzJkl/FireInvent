using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Database.Models;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class AppointmentService(AppDbContext context, AppointmentMapper mapper) : IAppointmentService
{
    public async Task<AppointmentModel?> GetAppointmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return appointment is null ? null : mapper.MapAppointmentToAppointmentModel(appointment);
    }

    public async Task<PagedResult<AppointmentModel>> GetAllAppointmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Appointments
            .OrderBy(a => a.ScheduledAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectAppointmentsToAppointmentModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<AppointmentModel> CreateAppointmentAsync(CreateOrUpdateAppointmentModel model, CancellationToken cancellationToken = default)
    {
        var appointment = mapper.MapCreateOrUpdateAppointmentModelToAppointment(model);

        await context.Appointments.AddAsync(appointment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        appointment = await context.Appointments
            .AsNoTracking()
            .FirstAsync(a => a.Id == appointment.Id, cancellationToken);

        return mapper.MapAppointmentToAppointmentModel(appointment);
    }

    public async Task<bool> UpdateAppointmentAsync(Guid id, CreateOrUpdateAppointmentModel model, CancellationToken cancellationToken = default)
    {
        var entity = await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (entity is null)
            return false;

        mapper.MapCreateOrUpdateAppointmentModelToAppointment(model, entity);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAppointmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TryDeleteEntityAsync(
            id,
            nameof(Appointment),
            context.Appointments,
            cancellationToken);
    }
}
