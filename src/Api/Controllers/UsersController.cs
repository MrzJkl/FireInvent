using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services.Keycloak;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("/users")]
public class UsersController(IKeycloakUserService userService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all users")]
    [EndpointDescription("Returns a list of all users.")]
    [ProducesResponseType<List<UserModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserModel>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get user by ID")]
    [EndpointDescription("Returns a user by its unique ID.")]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(id, cancellationToken);
        return user is null ? throw new NotFoundException() : (ActionResult<UserModel>)Ok(user);
    }
}
