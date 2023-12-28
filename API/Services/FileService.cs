using Azure.Storage.Blobs;
using API.Configurations;
using Microsoft.Extensions.Options;

namespace API.Services;

public class FileService
{
    private readonly BlobContainerClient filesContainer;

    public FileService(IOptions<BlobStorageSettings> blobStorageSettings)
    {
        var blobStorageConfig = blobStorageSettings.Value;
        var client = new BlobServiceClient(blobStorageConfig.ConnectionString);

        filesContainer = client.GetBlobContainerClient("images");
    }

    // TODO: Rewrite this function. Can we use our Result?
    public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
    {
        BlobResponseDto response = new();
        BlobClient client = filesContainer.GetBlobClient(blob.FileName);

        try
        {
            await using Stream? data = blob.OpenReadStream();
            await client.UploadAsync(data);
        }
        catch (Exception)
        {
            throw;
        }

        response.Status = $"File {blob.FileName} uploaded successfully";
        response.Error = false;
        response.Blob.Uri = client.Uri.AbsoluteUri;
        response.Blob.Name = client.Name;

        return response;
    }
}

public class BlobDto
{
    public string? Uri { get; set; }
    public string? Name { get; set; }
    public string? ContentType { get; set; }
    public Stream? Content { get; set; }
}

// TODO: Can we use Result for this?
public class BlobResponseDto
{
    public BlobResponseDto()
    {
        Blob = new BlobDto();
    }

    public string? Status { get; set; }
    public bool Error { get; set; }
    public BlobDto Blob { get; set; }
}
