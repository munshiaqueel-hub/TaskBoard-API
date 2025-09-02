using TaskBoard.Api.Models;
using TaskBoard.Api.Repositories;

namespace TaskBoard.Api.Services;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IColumnRepository _columns;
    private readonly IImageStorage _images;

    public TaskService(ITaskRepository tasks, IColumnRepository columns, IImageStorage images)
    {
        _tasks = tasks;
        _columns = columns;
        _images = images;
    }

    public async Task<BoardTask> CreateAsync(CreateTaskDto createTaskDto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(createTaskDto.Name)) throw new ArgumentException("Name required", nameof(createTaskDto.Name));
        var column = await _columns.GetAsync(createTaskDto.ColumnId, ct) ?? throw new InvalidOperationException("Column not found");
        var task = new BoardTask
        {
            Name = createTaskDto.Name.Trim(),
            Description = createTaskDto.Description?.Trim(),
            Deadline = createTaskDto.Deadline,
            ColumnId = column.Id,
            IsFavorite = createTaskDto.IsFavorite,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _tasks.AddAsync(task, ct);
        return task;
    }

    public Task<BoardTask?> GetAsync(TaskId id, CancellationToken ct) => _tasks.GetAsync(id, ct);

    public Task<IReadOnlyList<BoardTask?>> GetAllAsync(CancellationToken ct) => _tasks.GetAllAsync(ct);

    public async Task EditAsync(TaskId id, EditTaskDto editTaskDto, CancellationToken ct)
    {
        var task = await _tasks.GetAsync(id, ct) ?? throw new InvalidOperationException("Task not found");
        task.Name = string.IsNullOrWhiteSpace(editTaskDto.Name) ? task.Name : editTaskDto.Name.Trim();
        task.Description = editTaskDto.Description?.Trim();
        task.Deadline = editTaskDto.Deadline;
        task.IsFavorite = editTaskDto.IsFavorite;
        await _tasks.UpdateAsync(task, ct);
    }

    public Task DeleteAsync(TaskId id, CancellationToken ct) => _tasks.DeleteAsync(id, ct);

    public async Task MoveAsync(TaskId id, string toColumnId, CancellationToken ct)
    {
        var task = await _tasks.GetAsync(id, ct) ?? throw new InvalidOperationException("Task not found");
        var column = await _columns.GetAsync(toColumnId, ct) ?? throw new InvalidOperationException("Column not found");
        task.ColumnId = column.Id;
        await _tasks.UpdateAsync(task, ct);
    }

    public async Task<IReadOnlyList<BoardTask>> GetColumnTasksSortedAsync(Guid columnId, CancellationToken ct)
    {
        var tasks = await _tasks.GetByColumnAsync(columnId, ct);
        return tasks
            .OrderByDescending(t => t.IsFavorite)
            .ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<List<string>> AttachImageAsync(TaskId taskId, IFormFileCollection files, CancellationToken ct = default)
    {
        if (files.Count == 0)
        {
            throw new ArgumentException("Files not found");
        }
        var urls = new List<string>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();

            // Generate the storage path or URL
            var url = await _images.UploadAsync(file.FileName, stream, file.ContentType, ct);

            // Create TaskImage entity directly
            var taskImage = new TaskImage
            {
                Id = Guid.NewGuid(),
                TaskId = taskId.Value, // FK to parent task
                FileName = file.FileName,
                ContentType = file.ContentType,
                Url = url,
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            // Add directly to TaskImages DbSet
            _tasks.InsertImages(taskImage, ct);
            urls.Add(url);
        }
        await _tasks.SaveChanges(ct);
        return urls;
    }

}