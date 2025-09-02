using TaskBoard.Api.Models;

public interface IColumnService
{
    Task<IReadOnlyList<Column>> GetAllAsync(CancellationToken ct);
    
}