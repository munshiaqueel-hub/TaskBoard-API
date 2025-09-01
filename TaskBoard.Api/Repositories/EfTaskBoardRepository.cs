using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Models;

namespace TaskBoard.Api.Repositories;

public class EfTaskRepository : ITaskRepository
{
    private readonly TaskBoardDbContext _db;

    public EfTaskRepository(TaskBoardDbContext db)
    {
        _db = db;
    }

    public async Task<BoardTask?> GetAsync(TaskId id, CancellationToken ct)
    {
        return await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id.Value, ct);
    }

    public async Task AddAsync(BoardTask task, CancellationToken ct)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BoardTask task, CancellationToken ct)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(ct);
    }

    public void InsertImages(TaskImage taskImage, CancellationToken ct)
    {
        _db.TaskImages.Add(taskImage);
    }

    public async Task SaveChanges(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TaskId id, CancellationToken ct)
    {
        var task = await GetAsync(id, ct);
        if (task != null)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<BoardTask>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Tasks.Include(t => t.Images).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BoardTask>> GetByColumnAsync(Guid columnId, CancellationToken ct)
    {
        var list = await _db.Tasks.Include(t => t.Images).Where(t => t.ColumnId == columnId).ToListAsync();
        return list;
    }

}

// for in memory repo
// public sealed class InMemoryTaskRepository : ITaskRepository
// {
//     private readonly ConcurrentDictionary<Guid, BoardTask> _store = new();

//     public Task AddAsync(BoardTask task, CancellationToken ct)
//     {
//         _store[task.Id.Value] = Clone(task);
//         return Task.CompletedTask;
//     }

//     public Task DeleteAsync(TaskId id, CancellationToken ct)
//     {
//         _store.TryRemove(id.Value, out _);
//         return Task.CompletedTask;
//     }

//     public Task<BoardTask?> GetAsync(TaskId id, CancellationToken ct)
//     {
//         _store.TryGetValue(id.Value, out var found);
//         return Task.FromResult(found is null ? null : Clone(found));
//     }

//     public Task<IReadOnlyList<BoardTask>?> GetAllAsync(CancellationToken ct)
//     {
//         var list = _store.Values.Select(Clone).ToList();
//         return Task.FromResult<IReadOnlyList<BoardTask>?>(list);
//     }

//     public Task<IReadOnlyList<BoardTask>> GetByColumnAsync(string columnId, CancellationToken ct)
//     {
//         var list = _store.Values.Where(t => t.ColumnId == columnId)
//             .Select(Clone)
//             .ToList();
//         return Task.FromResult<IReadOnlyList<BoardTask>>(list);
//     }

//     public Task UpdateAsync(BoardTask task, CancellationToken ct)
//     {
//         _store[task.Id.Value] = Clone(task);
//         return Task.CompletedTask;
//     }

//     private static BoardTask Clone(BoardTask t) => new()
//     {
//         Id = new TaskId(t.Id.Value),
//         Name = t.Name,
//         Description = t.Description,
//         Deadline = t.Deadline,
//         ColumnId = t.ColumnId,
//         IsFavorite = t.IsFavorite,
//         ImageUrls = t.ImageUrls.ToList() 
//     };
// }