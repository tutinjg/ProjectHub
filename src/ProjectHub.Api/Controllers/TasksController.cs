using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHub.Application.Tasks.Commands.CreateTask;
using ProjectHub.Application.Tasks.Commands.UpdateTaskStatus;
using ProjectHub.Application.Tasks.Common;
using ProjectHub.Application.Tasks.Queries.GetTasksByProject;

namespace ProjectHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("by-project/{projectId:guid}")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TaskDto>>> GetByProject(Guid projectId)
    {
        var result = await _sender.Send(new GetTasksByProjectQuery(projectId));
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        await _sender.Send(new UpdateTaskStatusCommand(id, request.NewStatus, request.TargetOrder));
        return NoContent();
    }
}

public record UpdateTaskStatusRequest(Domain.Enums.TaskItemStatus NewStatus, int TargetOrder);