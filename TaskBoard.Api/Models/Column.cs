using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskBoard.Api.Models;

// public sealed record ColumnId(Guid Value)
// {
//     public static ColumnId New() => new(Guid.NewGuid());
//     public override string ToString() => Value.ToString();
// }

public sealed record TaskId(Guid Value)
{
    public static TaskId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public class Column
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<BoardTask> Tasks { get; set; } = new();
}
