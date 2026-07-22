using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;
using Xunit;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Tests.Infrastructure;

public class RepositoryTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly UserRepository _users;
    private readonly TaskRepository _tasks;

    public RepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _users = new UserRepository(_db);
        _tasks = new TaskRepository(_db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UserRepository_adds_and_gets_by_email()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@taskmanager.local",
            DisplayName = "Demo",
            PasswordHash = "hash",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _users.AddAsync(user);

        var found = await _users.GetByEmailAsync("demo@taskmanager.local");
        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task TaskRepository_scopes_get_by_id_to_owner()
    {
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        await _users.AddAsync(new User
        {
            Id = ownerId,
            Email = "owner@test.local",
            DisplayName = "Owner",
            PasswordHash = "h",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _users.AddAsync(new User
        {
            Id = otherId,
            Email = "other@test.local",
            DisplayName = "Other",
            PasswordHash = "h",
            CreatedAt = DateTimeOffset.UtcNow
        });

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Owned",
            Status = TaskStatus.Todo,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _tasks.AddAsync(task);

        var owned = await _tasks.GetByIdForUserAsync(task.Id, ownerId);
        var leaked = await _tasks.GetByIdForUserAsync(task.Id, otherId);

        Assert.NotNull(owned);
        Assert.Null(leaked);
    }

    [Fact]
    public async Task TaskRepository_lists_only_user_tasks_newest_first()
    {
        var userId = Guid.NewGuid();
        await _users.AddAsync(new User
        {
            Id = userId,
            Email = "list@test.local",
            DisplayName = "List",
            PasswordHash = "h",
            CreatedAt = DateTimeOffset.UtcNow
        });

        var older = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Older",
            Status = TaskStatus.Todo,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };
        var newer = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Newer",
            Status = TaskStatus.Done,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _tasks.AddAsync(older);
        await _tasks.AddAsync(newer);

        var list = await _tasks.GetByUserAsync(userId);

        Assert.Equal(2, list.Count);
        Assert.Equal("Newer", list[0].Title);
        Assert.Equal("Older", list[1].Title);
    }
}
