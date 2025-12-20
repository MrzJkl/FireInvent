using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("appointments")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all appointments")]
    [EndpointDescription("Returns a list of all appointments.")]
    [ProducesResponseType<List<AppointmentModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AppointmentModel>>> GetAll()
    {
        var appointments = await appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get appointment by ID")]
    [EndpointDescription("Returns an appointment by its unique ID.")]
    [ProducesResponseType<AppointmentModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentModel>> GetById(Guid id)
    {
        var appointment = await appointmentService.GetAppointmentByIdAsync(id);
        return appointment is null ? throw new NotFoundException() : Ok(appointment);
    }

    [HttpPost]
    [EndpointSummary("Create a new appointment")]
    [EndpointDescription("Creates a new appointment.")]
    [ProducesResponseType<AppointmentModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<AppointmentModel>> Create(CreateOrUpdateAppointmentModel model)
    {
        var created = await appointmentService.CreateAppointmentAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update an appointment")]
    [EndpointDescription("Updates an existing appointment.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateAppointmentModel model)
    {
        var success = await appointmentService.UpdateAppointmentAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete an appointment")]
    [EndpointDescription("Deletes an appointment by its unique ID. This will also delete all associated visits and visit items.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await appointmentService.DeleteAppointmentAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
