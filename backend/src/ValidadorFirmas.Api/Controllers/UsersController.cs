using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ValidadorFirmas.Application.Auth;
using ValidadorFirmas.Application.Dtos;
using ValidadorFirmas.Domain.Enums;

namespace ValidadorFirmas.Api.Controllers;

/// <summary>Administración de usuarios de la zona administrativa. Solo Administrador.</summary>
[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = nameof(UserRole.Administrador))]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _sender.Send(new GetUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Create(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), user);
    }
}
