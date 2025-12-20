using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using FireInvent.Shared.Exceptions;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointments;

    public AppointmentsController(IAppointmentService appointments)
    {
        _appointments = appointments;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentModel>>> GetAll()
    {
        var list = await _appointments.GetAllAppointmentsAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentModel>> GetById(Guid id)
    {
        var model = await _appointments.GetAppointmentByIdAsync(id);
        return model is null ? throw new NotFoundException() : Ok(model);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentModel>> Create([FromBody] CreateOrUpdateAppointmentModel model)
    {
        var created = await _appointments.CreateAppointmentAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateAppointmentModel model)
    {
        var success = await _appointments.UpdateAppointmentAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _appointments.DeleteAppointmentAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
