using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskStatus Status,
    DateTimeOffset? DueDate);
