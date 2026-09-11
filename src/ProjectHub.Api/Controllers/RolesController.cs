using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.Roles.Commands.CreateRole;

namespace ProjectHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateRoleCommand command)
    {
        var roleId = await _sender.Send(command);
        return Ok(roleId);
    }
}