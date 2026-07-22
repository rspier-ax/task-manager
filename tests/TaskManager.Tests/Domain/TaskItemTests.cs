using TaskManager.Domain.Entities;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;
using Xunit;

namespace TaskManager.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void New_task_defaults_status_to_Todo()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Write domain tests",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(DomainTaskStatus.Todo, task.Status);
        Assert.Null(task.Description);
        Assert.Null(task.DueDate);
    }

    [Fact]
    public void Task_can_use_each_status_value()
    {
        var statuses = new[]
        {
            DomainTaskStatus.Todo,
            DomainTaskStatus.InProgress,
            DomainTaskStatus.Done
        };

        foreach (var status in statuses)
        {
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Title = $"Status {status}",
                Status = status,
                Description = "optional notes",
                DueDate = DateTimeOffset.UtcNow.AddDays(1),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            Assert.Equal(status, task.Status);
            Assert.Equal("optional notes", task.Description);
            Assert.NotNull(task.DueDate);
        }
    }
}
