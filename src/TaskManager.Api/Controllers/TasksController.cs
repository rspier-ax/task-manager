using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Extensions;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskService _tasks;

    public TasksController(TaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> List(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var tasks = await _tasks.ListAsync(userId, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var task = await _tasks.GetByIdAsync(userId, id, cancellationToken);
        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var task = await _tasks.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskDto>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var task = await _tasks.UpdateAsync(userId, id, request, cancellationToken);
        return Ok(task);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _tasks.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }
}
