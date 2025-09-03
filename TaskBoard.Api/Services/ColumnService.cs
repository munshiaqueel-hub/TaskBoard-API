using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Api.Services;

public sealed class ColumnService : IColumnService
{
    private readonly IColumnRepository columnRepository;
    public ColumnService(IColumnRepository _columnRepository)
    {
        columnRepository = _columnRepository;
    }

    public async Task<Column> AddColumn(CreateColumnDto createColumnDto, CancellationToken ct)
    {
        var c = new Column { Id = new Guid(), Name = createColumnDto.Name };
        await columnRepository.AddAsync(c, ct);
        return c;
    }

    public async Task<IReadOnlyList<Column>> GetAllAsync(CancellationToken ct)
    {
        return await columnRepository.GetAllAsync(ct);
    }

    public async Task DeleteColumn(string columnId, CancellationToken ct)
    {
        await columnRepository.DeleteAsync(columnId, ct);
    }

    public async Task<Column?> GetByIdAsync(string columnId, CancellationToken ct)
    {
        return await columnRepository.GetAsync(columnId, ct);
    }
}