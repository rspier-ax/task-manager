using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskStatus Status,
    DateTimeOffset? DueDate);
