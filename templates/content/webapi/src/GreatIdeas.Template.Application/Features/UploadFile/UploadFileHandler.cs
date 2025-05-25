using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace GreatIdeas.Template.Application.Features.UploadFile;

public interface IUploadFileHandler : IApplicationHandler
{
    ValueTask<ErrorOr<ApiResponse>> UploadFile(string fileName, object id, HttpRequest httpRequest);
}

internal sealed class UploadFileHandler(
    ILogger<UploadFileHandler> logger,
    IPublishEndpoint publishEndpoint
) : IUploadFileHandler
{
    private static readonly ActivitySource ActivitySource = new(nameof(UploadFileHandler));

    public async ValueTask<ErrorOr<ApiResponse>> UploadFile(
        string fileName,
        object id,
        HttpRequest httpRequest
    )
    {
        // Start activity
        using var activity = ActivitySource.CreateActivity(nameof(UploadFile), ActivityKind.Server);
        activity?.Start();

        try
        {
            var uploadedFilePath = await UploadFile(httpRequest, fileName);

            // Upload blob file with event
            await publishEndpoint.Publish(new UploadFileEvent(id, uploadedFilePath, fileName));

            return new ApiResponse(Message: StatusLabels.UploadSuccess);
        }
        catch (Exception exception)
        {
            // Add event
            return exception.LogCritical(
                logger,
                activity: activity,
                message: StatusLabels.UnprocessableUploadError,
                entityName: nameof(File)
            );
        }
    }

    static async ValueTask<string> UploadFile(HttpRequest httpRequest, string filePath)
    {
        var form = await httpRequest.ReadFormAsync();

        var file = form.Files[0];
        var fileExtension = Path.GetExtension(file.FileName);
        var trustedFilename = $"{filePath.Split('.')[0]}{fileExtension}";
        var newFilePath = await SaveFormFileWithName(file, trustedFilename);

        // Resize image
        ResizeImage(newFilePath);

        return newFilePath;
    }

    static async ValueTask<string> SaveFormFileWithName(IFormFile file, string fileSaveName)
    {
        var filePath = GetOrCreateFilePath(fileSaveName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        return filePath;
    }

    static string GetOrCreateFilePath(string fileName, string filesDirectory = "tempfiles")
    {
        var filePath = Path.GetDirectoryName(fileName);
        var directoryPath = Path.Combine("wwwroot", filesDirectory, filePath!);
        Directory.CreateDirectory(directoryPath);
        return Path.Combine(directoryPath, Path.GetFileName(fileName)!);
    }

    static void ResizeImage(string imageFile)
    {
        using Image image = Image.Load(imageFile);
        image.Mutate(x => x.Resize(0, 248));
        image.SaveAsWebp(imageFile.Split('.')[0] + ".webp");
    }
}
