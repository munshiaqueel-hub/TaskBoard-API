using TaskBoard.Api.Models;

namespace TaskBoard.Api.Repositories;

public interface IColumnRepository
{
    Task<Column?> GetAsync(string id, CancellationToken ct);
    Task AddAsync(Column column, CancellationToken ct);
    Task<IReadOnlyList<Column>> GetAllAsync(CancellationToken ct);
}