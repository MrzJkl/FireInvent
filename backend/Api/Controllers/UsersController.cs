using FireInvent.Contract;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserModel>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }


    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await userService.CreateUserAsync(model);
        if (result.Succeeded)
            return Ok();

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return BadRequest(ModelState);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await userService.DeleteUserAsync(id);

        return result ? NoContent() : throw new NotFoundException();
    }
}
