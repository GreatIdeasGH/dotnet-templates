using Company.Project.Application.Common.Errors;

using Microsoft.AspNetCore.Http;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Company.Project.Application.Features.UploadFile;

public interface IUploadFileHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse<UploadFileResponse>>> UploadFileAsync(
        UploadParameters uploadParameters,
        HttpRequest httpRequest
    );

    ValueTask<ErrorOr<ApiResponse<UploadFileResponse>>> UploadFileAsync(
        UploadParameters uploadParameters,
        IFormFile file
    );
}

internal sealed class UploadFileHandler(
    ILogger<UploadFileHandler> logger,
    ExceptionNotificationService notificationService
) : IUploadFileHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(UploadFileHandler));

    public async ValueTask<ErrorOr<ApiResponse<UploadFileResponse>>> UploadFileAsync(
        UploadParameters uploadParameters,
        HttpRequest httpRequest
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(UploadFileAsync),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Check if the request contains files
            if (!httpRequest.HasFormContentType || !httpRequest.Form.Files.Any())
            {
                return DomainErrors.FileMissing("File", "Please upload a valid file");
            }

            // Generate a unique filename with the original extension
            var originalFile = httpRequest.Form.Files[0];
            var extension = Path.GetExtension(originalFile.FileName);
            var modifiedName = $"{uploadParameters.FileName}{extension}";

            var uploadedFilePath = await PreprocessFile(httpRequest, modifiedName);
            logger.LogInformation("File uploaded successfully: {FilePath}", uploadedFilePath);

            // Upload blob file with event
            // await publishEndpoint.Publish(new UploadFileEvent(id, uploadedFilePath, fileName));

            return new ApiResponse<UploadFileResponse>
            {
                Message = StatusLabels.UploadSuccess,
                Item = uploadedFilePath,
            };
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                notificationService,
                activity: activity,
                message: StatusLabels.UnprocessableUploadError,
                entityName: nameof(File)
            );
        }
    }

    public async ValueTask<ErrorOr<ApiResponse<UploadFileResponse>>> UploadFileAsync(
        UploadParameters uploadParameters,
        IFormFile file
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(UploadFileAsync),
            ActivityKind.Server
        );
        activity?.Start();

        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return DomainErrors.FileMissing("File", "Please upload a valid file");
            }

            // Generate a unique filename with the original extension
            var extension = Path.GetExtension(file.FileName);
            var modifiedName = $"{uploadParameters.FileName}{extension}";
            var trustedFilename = modifiedName.Replace(" ", "-").ToLowerInvariant();

            // Save and process the file
            var uploadedFilePath = await SaveFormFileWithName(file, trustedFilename);
            logger.LogInformation("File uploaded successfully: {FilePath}", uploadedFilePath);

            // Upload blob file with event
            // await publishEndpoint.Publish(new UploadFileEvent(id, uploadedFilePath, fileName));

            return new ApiResponse<UploadFileResponse>
            {
                Message = StatusLabels.UploadSuccess,
                Item = uploadedFilePath,
            };
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                notificationService,
                activity: activity,
                message: StatusLabels.UnprocessableUploadError,
                entityName: nameof(File)
            );
        }
    }

    private static async ValueTask<UploadFileResponse> PreprocessFile(
        HttpRequest httpRequest,
        string filePath
    )
    {
        // start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(PreprocessFile),
            ActivityKind.Server
        );
        activity?.Start();

        var form = await httpRequest.ReadFormAsync();

        var file = form.Files[0];
        var fileExtension = Path.GetExtension(file.FileName);
        var trustedFilename = $"{filePath.Split('.')[0]}{fileExtension}"
            .Replace(" ", "-")
            .ToLowerInvariant();

        var result = await SaveFormFileWithName(file, trustedFilename);
        return result;
    }

    private static async ValueTask<UploadFileResponse> SaveFormFileWithName(
        IFormFile file,
        string fileSaveName
    )
    {
        // start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(SaveFormFileWithName),
            ActivityKind.Server
        );
        activity?.Start();

        var filePath = GetOrCreateFilePath(fileSaveName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        // delete original file
        fileStream.Close();

        // Resize image
        var resizedPath = ResizeImage(filePath);

        var trimmedFilePath = TrimFirstDirectory(filePath);
        var trimmedResizedPath = TrimFirstDirectory(resizedPath);

        return new UploadFileResponse(true, trimmedFilePath, trimmedResizedPath);
    }

    private static string GetOrCreateFilePath(string fileName, string filesDirectory = "campaigns")
    {
        var filePath = Path.Combine(filesDirectory, fileName);
        var directoryPath = Path.GetDirectoryName(Path.Combine("wwwroot", filePath!));

        // Create directory with proper permissions
        Directory.CreateDirectory(directoryPath!);

        var fullPath = Path.Combine(directoryPath!, Path.GetFileName(fileName)!);
        return fullPath;
    }

    private static string ResizeImage(string imageFile)
    {
        // start activity
        using var activity = ActivitySource.CreateActivity(
            nameof(ResizeImage),
            ActivityKind.Server
        );
        activity?.Start();

        // Add thumbnail directory to existing directory
        var thumbnailDirectory = Path.Combine(Path.GetDirectoryName(imageFile)!, "thumbnails");
        Directory.CreateDirectory(thumbnailDirectory);
        var thumbnailFile = Path.Combine(thumbnailDirectory, Path.GetFileName(imageFile)!);
        var outputFile = thumbnailFile.Split('.')[0] + ".webp";

        // Resize image
        using Image image = Image.Load(imageFile);
        image.Mutate(x => x.Resize(0, 512));
        image.SaveAsWebp(outputFile);

        // return resized image with wwwroot
        return outputFile;
    }

    private static string TrimFirstDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var pathSeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var segments = path.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length <= 1)
            return path;

        return string.Join(Path.DirectorySeparatorChar.ToString(), segments.Skip(1));
    }
}
