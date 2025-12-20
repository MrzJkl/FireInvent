using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using FireInvent.Shared.Exceptions;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("visits")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _visits;

    public VisitsController(IVisitService visits)
    {
        _visits = visits;
    }

    [HttpGet]
    public async Task<ActionResult<List<VisitModel>>> GetAll()
    {
        var list = await _visits.GetAllVisitsAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VisitModel>> GetById(Guid id)
    {
        var model = await _visits.GetVisitByIdAsync(id);
        return model is null ? throw new NotFoundException() : Ok(model);
    }

    [HttpPost]
    public async Task<ActionResult<VisitModel>> Create([FromBody] CreateOrUpdateVisitModel model)
    {
        var created = await _visits.CreateVisitAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateVisitModel model)
    {
        var success = await _visits.UpdateVisitAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _visits.DeleteVisitAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
