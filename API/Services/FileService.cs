using Azure.Storage.Blobs;
using API.Configurations;
using Microsoft.Extensions.Options;
using API.Models.DTO;

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

    public async Task<Result<BlobDto>> UploadAsync(IFormFile file)
    {
        try
        {
            string format = Path.GetExtension(file.FileName);
            BlobClient client = filesContainer.GetBlobClient($"{Guid.NewGuid}{format}");

            await using Stream? data = file.OpenReadStream();
            await client.UploadAsync(data, true);

            return new SuccessResult<BlobDto>(
                new BlobDto()
                {
                    Uri = client.Uri.AbsoluteUri,
                    Name = client.Name,
                }
            );
        }
        catch (Exception)
        {
            return new ErrorResult<BlobDto>("Could not upload a file");
        }
    }
}

public class BlobDto
{
    public string Uri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public Stream? Content { get; set; }
}
