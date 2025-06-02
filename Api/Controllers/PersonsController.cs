using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("persons")]
    public class PersonsController(PersonService personService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<PersonModel>>> GetAll()
        {
            var persons = await personService.GetAllPersonsAsync();
            return Ok(persons);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PersonModel>> GetById(Guid id)
        {
            var person = await personService.GetPersonByIdAsync(id);
            if (person is null)
                return NotFound();

            return Ok(person);
        }

        [HttpPost]
        public async Task<ActionResult<PersonModel>> Create(PersonModel model)
        {
            try
            {
                var created = await personService.CreatePersonAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, PersonModel model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch.");

            try
            {
                var success = await personService.UpdatePersonAsync(model);
                return success ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await personService.DeletePersonAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
