using TaskBoard.Api.Models;

namespace TaskBoard.Api.Repositories;

public interface ITaskRepository
{
    Task<BoardTask?> GetAsync(TaskId id, CancellationToken ct);
    Task<IReadOnlyList<BoardTask?>> GetAllAsync(CancellationToken ct);
    Task AddAsync(BoardTask task, CancellationToken ct);
    Task UpdateAsync(BoardTask task, CancellationToken ct);
    Task DeleteAsync(TaskId id, CancellationToken ct);
    Task<IReadOnlyList<BoardTask>> GetByColumnAsync(Guid columnId, CancellationToken ct);
    void InsertImages(TaskImage taskImage, CancellationToken ct);
    public Task SaveChanges(CancellationToken ct);

}