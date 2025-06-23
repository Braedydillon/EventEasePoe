
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

public class BlobService
{
    private readonly string _connectionString;

    public BlobService(IConfiguration configuration)
    {
        _connectionString = configuration["AzureBlobStorage:ConnectionString"];
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
    {
        var containerClient = new BlobContainerClient(_connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync();
        await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
        return containerClient;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string containerName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(file.FileName));

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string blobUrl, string containerName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobName = Path.GetFileName(new Uri(blobUrl).AbsolutePath);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
