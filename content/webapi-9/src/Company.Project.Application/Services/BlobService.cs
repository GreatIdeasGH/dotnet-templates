using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Company.Project.Application.Common.Options;
using Microsoft.Extensions.Options;
using CrudLogDefinitions = Company.Project.Application.Common.Extensions.CrudLogDefinitions;
using LogDefinitions = Company.Project.Application.Common.Extensions.LogDefinitions;

namespace Company.Project.Application.Services;

internal sealed class BlobService : IBlobService
{
    private readonly AzureSettings _azureSettings;
    private readonly ILogger<BlobService> _logger;

    private BlobContainerClient _blobContainerClient = null!;

    public string BlobUrl { get; set; } = null!;

    private static readonly ActivitySource ActivitySource = new(nameof(BlobService));

    public BlobService(IOptionsMonitor<ApplicationSettings> options, ILogger<BlobService> logger)
    {
        _azureSettings = options.CurrentValue.AzureSettings;
        _logger = logger;

        Setup(_azureSettings.BlobStorage.ContainerName);
    }

    public async ValueTask<List<string>> ListAllFiles(string level = "")
    {
        var counter = 0;
        List<string> blobs = [];
        await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(prefix: level))
        {
            blobs.Add(blobItem.Name);
            counter++;
        }

        LogDefinitions.LogToInfo(_logger, $"Total files: {counter}");
        return blobs;
    }

    public async ValueTask DownloadFiles(string downloadPath, string level, bool isTrial = false)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DownloadFiles),
            ActivityKind.Consumer
        );
        activity?.Start();

        OtelConstants.AddInfoEvent(activity, $"Downloading {level} resources to {downloadPath}...");

        await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(prefix: level))
        {
            var filePath = Path.Combine(downloadPath, blobItem.Name);

            // Download only trial-level files
            if (isTrial)
            {
                // Replace the word 'level' with 'trial-level' in filePath
                filePath = filePath.Replace("level", "trial-level");
                await GetBlobFiles(blobItem, filePath);
            }
            else
            {
                await GetBlobFiles(blobItem, filePath);
            }

            CrudLogDefinitions.Download(_logger, "Downloaded", blobItem.Name);
        }

        OtelConstants.AddInfoEvent(activity, "Resources download complete");
    }

    private async Task<string?> GetBlobFiles(BlobItem blobItem, string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(directory!);

        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobItem.Name);
        BlobDownloadInfo download = await blobClient.DownloadAsync();

        await using (FileStream file = new(filePath, FileMode.Create, FileAccess.Write))
        {
            await download.Content.CopyToAsync(file);
        }

        return directory;
    }

    public async ValueTask<ApiResponse<FileProperties>> GetDownloadUri(
        string fileName,
        CancellationToken cancellationToken
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(GetDownloadUri),
            ActivityKind.Consumer
        );
        activity?.Start();

        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        if (!await blobClient.ExistsAsync(cancellationToken: cancellationToken))
        {
            return new ApiResponse<FileProperties>
            {
                Message = "File does not exist for this level.",
            };
        }

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        // Convert the ContentHash (byte array) to a base64 string to represent the MD5 hash
        string contentMd5 = Convert.ToBase64String(properties.Value.ContentHash);

        var result = new ApiResponse<FileProperties>
        {
            Item = new FileProperties
            {
                ContentLength = properties.Value.ContentLength,
                MD5 = contentMd5,
                LastModified = properties.Value.LastModified,
                ResourceUri = blobClient.Uri.OriginalString,
            },
            Message = $"Download URI for {fileName} was generated successfully.",
        };

        OtelConstants.AddInfoEvent(activity, $"Generated URI for {fileName}");
        return result;
    }

    public async ValueTask DeleteFolder(string folderName)
    {
        var blobs = _blobContainerClient.GetBlobs(prefix: folderName);

        foreach (var blob in blobs)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blob.Name);
            await blobClient.DeleteIfExistsAsync();
        }
    }

    public async ValueTask DeleteFile(string fileName)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteFile),
            ActivityKind.Consumer
        );
        activity?.Start();

        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async ValueTask<bool> DeleteFile(string container, string fileName)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(DeleteFile),
            ActivityKind.Consumer
        );
        activity?.Start();

        Setup(container);

        BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
        var result = await blobClient.DeleteIfExistsAsync();
        return result.Value;
    }

    public async ValueTask<BlobContentInfo?> UploadFile(string filePath, string fileName)
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(UploadFile),
            ActivityKind.Consumer
        );
        activity?.Start();

        BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
        BlobHttpHeaders headers = new() { ContentType = filePath.GetContentType() };
        using FileStream uploadFileStream = File.OpenRead(filePath);
        var result = await blobClient.UploadAsync(uploadFileStream, httpHeaders: headers);
        uploadFileStream.Close();

        OtelConstants.AddSuccessEvent(_logger, activity, $"Uploaded {fileName} to blob storage.");
        return result.Value;
    }

    public async ValueTask<BlobContentInfo?> UploadFile(
        string container,
        string filePath,
        string fileName
    )
    {
        using var activity = ActivitySource.CreateActivity(
            nameof(UploadFile),
            ActivityKind.Consumer
        );
        activity?.Start();

        Setup(container);

        BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
        BlobHttpHeaders headers = new() { ContentType = filePath.GetContentType() };
        using FileStream uploadFileStream = File.OpenRead(filePath);
        var result = await blobClient.UploadAsync(uploadFileStream, httpHeaders: headers);
        uploadFileStream.Close();

        OtelConstants.AddSuccessEvent(_logger, activity, $"Uploaded {fileName} to blob storage.");
        return result.Value;
    }

    public async ValueTask<BlobProperties> CopyBlobAsync(
        string sourceContainerName,
        string sourceBlobName,
        string destContainerName,
        string destBlobName
    )
    {
        // Create a BlobServiceClient object for source and destination
        Setup(sourceContainerName);

        // Get a reference to the source container and blob
        BlobContainerClient sourceContainerClient = _blobContainerClient;
        BlobClient sourceBlobClient = sourceContainerClient.GetBlobClient(sourceBlobName);

        // Get a reference to the destination container and blob
        Setup(destContainerName);
        BlobContainerClient destContainerClient = _blobContainerClient;

        BlobClient destBlobClient = destContainerClient.GetBlobClient(destBlobName);

        // Start the copy operation
        CopyFromUriOperation copyOperation = await destBlobClient.StartCopyFromUriAsync(
            sourceBlobClient.Uri
        );

        // Wait for the copy operation to complete
        await copyOperation.WaitForCompletionAsync();

        // Get the blob's properties and display the copy status
        BlobProperties properties = await destBlobClient.GetPropertiesAsync();
        LogDefinitions.LogToInfo(_logger, $"Copy status: {properties.CopyStatus}");
        return properties;
    }

    private void Setup(string containerName)
    {
        // Get credentials from environment variables
        var connectionString =
            $"DefaultEndpointsProtocol=https;AccountName={_azureSettings.BlobStorage.AccountName};AccountKey={_azureSettings.BlobStorage.AccountKey};EndpointSuffix=core.windows.net";
        BlobServiceClient blobServiceClient = new(connectionString);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        BlobUrl = _blobContainerClient.Uri.Scheme + "://" + _blobContainerClient.Uri.Host;
    }
}

public sealed record FileProperties
{
    public long ContentLength { get; set; }

    [JsonPropertyName("md5")]
    public string MD5 { get; set; } = null!;
    public DateTimeOffset LastModified { get; set; }
    public string ResourceUri { get; set; } = string.Empty;
}
