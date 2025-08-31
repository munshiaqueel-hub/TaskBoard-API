namespace TaskBoard.Api.Models;

public class TaskImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    public BoardTask Task { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public DateTimeOffset? CreatedAt {get;set;}

}