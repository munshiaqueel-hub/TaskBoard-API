// using System.Collections.Concurrent;
// using TaskBoard.Api.Models;

// namespace TaskBoard.Api.Repositories;

// public sealed class InMemoryColumnRepository : IColumnRepository
// {
//     private readonly ConcurrentDictionary<Guid, Column> _store = new();

//     public Task AddAsync(Column column, CancellationToken ct)
//     {
//         GuidHelper.GetParsedGuidFromString(column.Id, out Guid guid);
//         _store[guid] = new Column { Id = column.Id, Name = column.Name };
//         return Task.CompletedTask;
//     }

//     public Task<IReadOnlyList<Column>> GetAllAsync(CancellationToken ct)
//     {
//         var list = _store.Values.Select(c => new Column { Id = c.Id, Name = c.Name }).ToList();
//         return Task.FromResult<IReadOnlyList<Column>>(list);
//     }

//     public Task<Column?> GetAsync(string id, CancellationToken ct)
//     {
//         GuidHelper.GetParsedGuidFromString(id, out Guid guid);
//         _store.TryGetValue(guid, out var found);
//         return Task.FromResult(found is null ? null : new Column { Id = found.Id, Name = found.Name });
//     }
// }
