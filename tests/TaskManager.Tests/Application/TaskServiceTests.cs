using NSubstitute;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Tests.Application;

public class TaskServiceTests
{
    private readonly ITaskRepository _tasks = Substitute.For<ITaskRepository>();
    private readonly TaskService _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskServiceTests()
    {
        _sut = new TaskService(_tasks);
    }

    [Fact]
    public async Task CreateAsync_persists_task_for_user()
    {
        TaskItem? saved = null;
        _tasks.AddAsync(Arg.Do<TaskItem>(t => saved = t), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var dto = await _sut.CreateAsync(
            _userId,
            new CreateTaskRequest("Buy milk", "2%", TaskStatus.Todo, null));

        Assert.NotNull(saved);
        Assert.Equal(_userId, saved!.UserId);
        Assert.Equal("Buy milk", saved.Title);
        Assert.Equal(TaskStatus.Todo, saved.Status);
        Assert.Equal(saved.Id, dto.Id);
        Assert.Equal("Buy milk", dto.Title);
    }

    [Fact]
    public async Task ListAsync_returns_mapped_dtos()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            Title = "One",
            Status = TaskStatus.InProgress,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _tasks.GetByUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new List<TaskItem> { task });

        var result = await _sut.ListAsync(_userId);

        Assert.Single(result);
        Assert.Equal("One", result[0].Title);
        Assert.Equal(TaskStatus.InProgress, result[0].Status);
    }

    [Fact]
    public async Task UpdateAsync_updates_owned_task()
    {
        var taskId = Guid.NewGuid();
        var existing = new TaskItem
        {
            Id = taskId,
            UserId = _userId,
            Title = "Old",
            Status = TaskStatus.Todo,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _tasks.GetByIdForUserAsync(taskId, _userId, Arg.Any<CancellationToken>()).Returns(existing);

        var dto = await _sut.UpdateAsync(
            _userId,
            taskId,
            new UpdateTaskRequest("New", null, TaskStatus.Done, null));

        Assert.Equal("New", dto.Title);
        Assert.Equal(TaskStatus.Done, dto.Status);
        await _tasks.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_removes_owned_task()
    {
        var taskId = Guid.NewGuid();
        var existing = new TaskItem { Id = taskId, UserId = _userId, Title = "Gone" };
        _tasks.GetByIdForUserAsync(taskId, _userId, Arg.Any<CancellationToken>()).Returns(existing);

        await _sut.DeleteAsync(_userId, taskId);

        await _tasks.Received(1).DeleteAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_throws_when_task_not_owned()
    {
        var taskId = Guid.NewGuid();
        _tasks.GetByIdForUserAsync(taskId, _userId, Arg.Any<CancellationToken>())
            .Returns((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetByIdAsync(_userId, taskId));
    }

    [Fact]
    public async Task CreateAsync_rejects_empty_title()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.CreateAsync(
                _userId,
                new CreateTaskRequest("   ", null, TaskStatus.Todo, null)));
    }
}
