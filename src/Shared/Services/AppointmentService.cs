using FireInvent.Database;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class AppointmentService(AppDbContext context, AppointmentMapper mapper) : IAppointmentService
{
    public async Task<AppointmentModel?> GetAppointmentByIdAsync(Guid id)
    {
        var appointment = await context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment is null ? null : mapper.MapAppointmentToAppointmentModel(appointment);
    }

    public async Task<List<AppointmentModel>> GetAllAppointmentsAsync()
    {
        var appointments = await context.Appointments
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapAppointmentsToAppointmentModels(appointments);
    }

    public async Task<AppointmentModel> CreateAppointmentAsync(CreateOrUpdateAppointmentModel model)
    {
        var appointment = mapper.MapCreateOrUpdateAppointmentModelToAppointment(model);

        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();

        appointment = await context.Appointments
            .AsNoTracking()
            .FirstAsync(a => a.Id == appointment.Id);

        return mapper.MapAppointmentToAppointmentModel(appointment);
    }

    public async Task<bool> UpdateAppointmentAsync(Guid id, CreateOrUpdateAppointmentModel model)
    {
        var entity = await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null)
            return false;

        mapper.MapCreateOrUpdateAppointmentModelToAppointment(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAppointmentAsync(Guid id)
    {
        var entity = await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null)
            return false;

        context.Appointments.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
