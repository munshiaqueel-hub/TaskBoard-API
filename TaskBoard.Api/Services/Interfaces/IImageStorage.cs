namespace TaskBoard.Api.Services;

public interface IImageStorage
{
    Task<string> UploadAsync(string fileName, Stream content, string? contentType, CancellationToken ct);
}