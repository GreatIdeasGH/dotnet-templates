using Azure.Storage.Blobs.Models;

namespace Company.Project.Application.Abstractions.Services;

public interface IBlobService
{
    ValueTask<List<string>> ListAllFiles(string level = "");
    ValueTask DownloadFiles(string downloadPath, string level, bool isTrial = false);
    ValueTask<ApiResponse<FileProperties>> GetDownloadUri(
        string fileName,
        CancellationToken cancellationToken
    );
    ValueTask DeleteFolder(string folderName);
    ValueTask DeleteFile(string fileName);
    ValueTask<bool> DeleteFile(string container, string fileName);
    ValueTask<BlobContentInfo?> UploadFile(string filePath, string fileName);
    ValueTask<BlobContentInfo?> UploadFile(string container, string filePath, string fileName);
    ValueTask<BlobProperties> CopyBlobAsync(
        string sourceContainerName,
        string sourceBlobName,
        string destContainerName,
        string destBlobName
    );
    string BlobUrl { get; }
}
