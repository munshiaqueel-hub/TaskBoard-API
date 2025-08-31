using TaskBoard.Api.Models;

namespace TaskBoard.Api.Services;

public interface ITaskService
{
    Task<BoardTask> CreateAsync(CreateTaskDto createTaskDto, CancellationToken ct);
    Task<BoardTask?> GetAsync(TaskId id, CancellationToken ct);
    Task<IReadOnlyList<BoardTask>> GetAllAsync(CancellationToken ct);
    public Task EditAsync(TaskId id, EditTaskDto editTaskDto, CancellationToken ct);
    Task DeleteAsync(TaskId id, CancellationToken ct);
    Task MoveAsync(TaskId id, string toColumnId, CancellationToken ct);
    Task<IReadOnlyList<BoardTask>> GetColumnTasksSortedAsync(Guid columnId, CancellationToken ct);
    Task<List<string>> AttachImageAsync(TaskId id, IFormFileCollection files, CancellationToken ct);
}