namespace Company.Project.Application.Services;

using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public static class AzureBlobService
{
    private static readonly BlobServiceClient BlobServiceClient = new(
        Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
    );

    private const string ContainerName = "resources";

    public static async Task ListAllFiles(string level = "")
    {
        BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(
            ContainerName
        );
        var counter = 0;
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: level))
        {
            Console.WriteLine("\t" + blobItem.Name);
            counter++;
        }

        Console.WriteLine($"*** Total files: {counter} ***");
    }

    public static async Task DownloadFiles(string downloadPath, string level)
    {
        // Create a local directory to store the downloaded files
        BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(
            ContainerName
        );

        var blobItems = containerClient.GetBlobsAsync(prefix: level);

        await foreach (var blobItem in blobItems)
        {
            var filepath = Path.Combine(downloadPath, blobItem.Name);
            var directory = Path.GetDirectoryName(filepath);
            Directory.CreateDirectory(directory!);

            BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            await using (FileStream file = new(filepath, FileMode.Create, FileAccess.Write))
            {
                await download.Content.CopyToAsync(file);
            }

            Console.WriteLine("\tDownloaded: {0}", blobItem.Name);
        }

        Console.WriteLine("Download complete");
    }

    public static async Task DeleteFolder(string folderName)
    {
        BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(
            ContainerName
        );

        var blobs = containerClient.GetBlobs(prefix: folderName);

        foreach (var blob in blobs)
        {
            var blobClient = containerClient.GetBlobClient(blob.Name);
            await blobClient.DeleteIfExistsAsync();
        }

        Console.WriteLine("Deleted folder: {0}", folderName);
    }

    public static async Task DeleteFile(string fileName)
    {
        BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(
            ContainerName
        );
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
        Console.WriteLine("Deleted file: {0}", fileName);
    }

    public static async Task UploadFile(string filePath, string fileName)
    {
        BlobContainerClient containerClient = BlobServiceClient.GetBlobContainerClient(
            ContainerName
        );
        BlobClient blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(filePath, true);
        Console.WriteLine("Uploaded file: {0}", fileName);
    }
}
