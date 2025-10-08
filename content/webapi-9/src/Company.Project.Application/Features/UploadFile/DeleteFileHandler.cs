using Company.Project.Application.Common.Errors;

namespace Company.Project.Application.Features.UploadFile;

public interface IDeleteFileHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> HandleAsync(
        string filePath,
        CancellationToken cancellationToken
    );
}

internal sealed class DeleteFileHandler(ILogger<DeleteFileHandler> logger) : IDeleteFileHandler
{
    public async ValueTask<ErrorOr<ApiResponse>> HandleAsync(
        string filePath,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // Delete file from wwwroot
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
            }
            else
            {
                logger.LogWarning("File not found: {FilePath}", fullPath);
                return DomainErrors.FileNotFound(filePath);
            }

            return new ApiResponse(Message: "File deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file.");
            return DomainErrors.DeleteFailed("Failed to delete file", "File");
        }
    }
}
