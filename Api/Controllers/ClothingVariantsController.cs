using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("clothingVariants")]
    public class ClothingVariantsController(ClothingVariantService variantService) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<List<ClothingVariantModel>>> GetAll()
        {
            var variants = await variantService.GetAllVariantsAsync();
            return Ok(variants);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClothingVariantModel>> GetById(Guid id)
        {
            var variant = await variantService.GetVariantByIdAsync(id);
            if (variant is null)
                return NotFound();

            return Ok(variant);
        }

        [HttpPost]
        public async Task<ActionResult<ClothingVariantModel>> Create(ClothingVariantModel model)
        {
            var created = await variantService.CreateVariantAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, ClothingVariantModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            var success = await variantService.UpdateVariantAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await variantService.DeleteVariantAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
