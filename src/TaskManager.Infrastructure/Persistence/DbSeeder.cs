using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Infrastructure.Persistence;

public static class DbSeeder
{
    public const string DemoEmail = "demo@taskmanager.local";
    public const string DemoPassword = "Demo123!";

    public static async Task EnsureDatabaseAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = DemoEmail,
            DisplayName = "Demo User",
            PasswordHash = hasher.Hash(DemoPassword),
            CreatedAt = now
        };

        db.Users.Add(user);
        db.Tasks.AddRange(
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Review interview requirements",
                Description = "Skim README and ADRs before the call.",
                Status = TaskStatus.Done,
                DueDate = now.AddDays(-1),
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-1)
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Prepare demo walkthrough",
                Description = "Login, CRUD tasks, show 401 without token.",
                Status = TaskStatus.InProgress,
                DueDate = now.AddDays(1),
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Explain GenAI validation",
                Description = "Ownership filters, password hashing, seed credentials.",
                Status = TaskStatus.Todo,
                DueDate = now.AddDays(2),
                CreatedAt = now,
                UpdatedAt = now
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
