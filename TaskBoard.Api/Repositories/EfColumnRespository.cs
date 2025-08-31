using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Models;

namespace TaskBoard.Api.Repositories;

public class EfColumnRepository : IColumnRepository
{
    private readonly TaskBoardDbContext _db;

    public EfColumnRepository(TaskBoardDbContext db) => _db = db;

    public async Task AddAsync(Column column, CancellationToken ct)
    {
        _db.Columns.Add(column);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Column?> GetAsync(string columnId, CancellationToken ct)
        => await _db.Columns.Include(c => c.Tasks)
                            .FirstOrDefaultAsync(c => c.Id.ToString() == columnId, ct);

    public async Task<IReadOnlyList<Column>> GetAllAsync(CancellationToken ct)
        => await _db.Columns.ToListAsync(ct);
}
