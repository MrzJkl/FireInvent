using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("/users")]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserModel>>> GetAll()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get user by ID", Description = "Returns a user by its unique ID.")]
    [SwaggerResponse(200, "User found", typeof(StorageLocationModel))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult<UserModel>> GetById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        return user is null ? throw new NotFoundException() : (ActionResult<UserModel>)Ok(user);
    }
}
