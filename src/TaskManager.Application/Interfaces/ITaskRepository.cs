using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
}
