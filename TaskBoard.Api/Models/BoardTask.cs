namespace TaskBoard.Api.Models;

public class BoardTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset? Deadline { get; set; }
    public bool IsFavorite { get; set; }

    public Guid ColumnId { get; set; }
    public Column Column { get; set; }

    public List<TaskImage> Images { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}
