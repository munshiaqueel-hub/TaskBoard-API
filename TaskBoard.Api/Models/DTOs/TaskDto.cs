namespace TaskBoard.Api.Models;

public record CreateTaskDto(string Name, string? Description, DateTimeOffset? Deadline, string ColumnId, bool IsFavorite);
public record EditTaskDto(string Name, string? Description, DateTimeOffset? Deadline, bool IsFavorite);
public record ColumnsSwitchDto(Guid Id, Guid ColumnId);
public record TaskDto(Guid Id, string Title, string Description, DateTimeOffset? Deadline, Guid ColumnId, bool? IsFavorite);
