using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("clothingProducts")]
    public class ClothingProductsController(ClothingProductService productService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ClothingProductModel>>> GetAll()
        {
            var products = await productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClothingProductModel>> GetById(Guid id)
        {
            var product = await productService.GetProductByIdAsync(id);
            if (product is null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ClothingProductModel>> Create(ClothingProductModel model)
        {
            var created = await productService.CreateProductAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, ClothingProductModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            try
            {
                var success = await productService.UpdateProductAsync(model);
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
            var success = await productService.DeleteProductAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
