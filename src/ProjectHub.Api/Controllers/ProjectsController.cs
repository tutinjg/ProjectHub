using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.Projects.Commands.CreateProject;
using ProjectHub.Application.Projects.Common;
using ProjectHub.Application.Projects.Queries.GetProjects;

namespace ProjectHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectDto>>> GetAll()
    {
        var result = await _sender.Send(new GetProjectsQuery());
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
}