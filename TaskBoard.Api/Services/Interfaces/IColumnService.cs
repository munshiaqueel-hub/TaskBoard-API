using TaskBoard.Api.Models;

public interface IColumnService
{
    Task<IReadOnlyList<Column>> GetAllAsync(CancellationToken ct);
    Task<Column> AddColumn(CreateColumnDto createColumnDto, CancellationToken ct);
    Task DeleteColumn(string columnId, CancellationToken ct);
    Task<Column> GetByIdAsync(string columnId, CancellationToken ct);
}