namespace BlazorApp1.Data;

using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BlobService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobService(IConfiguration configuration)
    {
        _connectionString = configuration["BlobStorage:ConnectionString"];
        _containerName = configuration["BlobStorage:ContainerName"];
    }

    public async Task<IEnumerable<string>> ListBlobsAsync()
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        var blobs = new List<string>();
    
        await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
        {
            blobs.Add(blobItem.Name);
        }

        return blobs;
    }

}