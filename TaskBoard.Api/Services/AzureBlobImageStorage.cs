using Azure.Storage.Blobs;
using Azure;
using Azure.Storage.Sas;

namespace TaskBoard.Api.Services;

public sealed class AzureBlobImageStorage : IImageStorage
{
    private readonly BlobContainerClient _container;

    public AzureBlobImageStorage(string connectionString, string containerName)
    {
        _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadAsync(string fileName, Stream content, string? contentType, CancellationToken ct)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = _container.GetBlobClient(fileName);
        var headers = new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType ?? "application/octet-stream" };
        content.Position = 0;
        await blob.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobUploadOptions { HttpHeaders = headers }, ct);
         // Generate SAS URL (valid for 1 hour)
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            Resource = "b", // blob
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Allow read access
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blob.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
            
    }
}