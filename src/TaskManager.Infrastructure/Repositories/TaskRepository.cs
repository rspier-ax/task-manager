using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskItem>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);

        // SQLite cannot ORDER BY DateTimeOffset in SQL; sort in memory.
        return tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public Task<TaskItem?> GetByIdForUserAsync(
        Guid taskId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _db.Tasks.FirstOrDefaultAsync(
            t => t.Id == taskId && t.UserId == userId,
            cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
