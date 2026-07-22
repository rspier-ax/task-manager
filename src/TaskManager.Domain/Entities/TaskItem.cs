using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
