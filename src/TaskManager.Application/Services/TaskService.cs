using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.Services;

public class TaskService
{
    private readonly ITaskRepository _tasks;

    public TaskService(ITaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<TaskDto> CreateAsync(
        Guid userId,
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var title = ValidateTitle(request.Title);
        EnsureValidStatus(request.Status);

        var now = DateTimeOffset.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Description = NormalizeDescription(request.Description),
            Status = request.Status,
            DueDate = request.DueDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _tasks.AddAsync(task, cancellationToken);
        return ToDto(task);
    }

    public async Task<IReadOnlyList<TaskDto>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _tasks.GetByUserAsync(userId, cancellationToken);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<TaskDto> GetByIdAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        return ToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        var title = ValidateTitle(request.Title);
        EnsureValidStatus(request.Status);

        task.Title = title;
        task.Description = NormalizeDescription(request.Description);
        task.Status = request.Status;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _tasks.UpdateAsync(task, cancellationToken);
        return ToDto(task);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskAsync(userId, taskId, cancellationToken);
        await _tasks.DeleteAsync(task, cancellationToken);
    }

    private async Task<TaskItem> GetOwnedTaskAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdForUserAsync(taskId, userId, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }

        return task;
    }

    private static string ValidateTitle(string? title)
    {
        var trimmed = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ValidationException("Title is required.");
        }

        return trimmed;
    }

    private static void EnsureValidStatus(TaskStatus status)
    {
        if (!Enum.IsDefined(typeof(TaskStatus), status))
        {
            throw new ValidationException("Status is invalid.");
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmed = description.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static TaskDto ToDto(TaskItem task) =>
        new(
            task.Id,
            task.UserId,
            task.Title,
            task.Description,
            task.Status,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt);
}
