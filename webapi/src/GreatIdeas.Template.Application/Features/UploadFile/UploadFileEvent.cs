using GreatIdeas.Template.Application.Common;
using GreatIdeas.Template.Application.Common.Errors;
using CrudLogDefinitions = GreatIdeas.Template.Application.Common.Extensions.CrudLogDefinitions;
using LogDefinitions = GreatIdeas.Template.Application.Common.Extensions.LogDefinitions;

namespace GreatIdeas.Template.Application.Features.UploadFile;

public sealed record UploadFileEvent(object Identifier, string FilePath, string FileName)
    : EventBase;

public sealed class UploadFileConsumer(ILogger<UploadFileConsumer> logger, IBlobService blobService)
    : IConsumer<UploadFileEvent>
{
    private static readonly ActivitySource ActivitySource = new(nameof(UploadFileConsumer));
    private static readonly string ContainerName = "flashcards";

    public async Task Consume(ConsumeContext<UploadFileEvent> context)
    {
        using var activity = ActivitySource.CreateActivity(
            "UploadFile-Consume",
            ActivityKind.Consumer
        );
        activity?.Start();

        // Verify existing path
        var blobPath = context.Message.FilePath;
        if (!File.Exists(blobPath))
        {
            CrudLogDefinitions.NotFound(logger, context.Message.FilePath, "File");
            var error = DomainErrors.FileNotFound("File not found to be uploaded");
            OtelConstants.AddErrorEvent(activity, error);
            // send email to the admin
            return;
        }

        // Upload file to storage
        var blobClient = await blobService.UploadFile(
            ContainerName,
            blobPath,
            context.Message.FileName
        );

        if (blobClient is null)
        {
            logger.LogError("Failed to upload {File} to blob storage", blobPath);
            var error = DomainErrors.UploadFailed($"Failed to upload file: {blobPath}", "File");
            OtelConstants.AddErrorEvent(activity, error);
            return;
        }

        // Upload resized file
        var resizedBlob = blobPath.Split('.')[0] + ".webp";
        var resizedFileName = context.Message.FileName.Split('.')[0] + ".webp";

        await blobService.UploadFile(ContainerName, resizedBlob, resizedFileName);

        LogDefinitions.LogToInfo(logger, $"Successfully uploaded {context.Message.FileName} files to Blob Storage");
        activity?.SetTag("identifier", context.Message.Identifier);
        OtelConstants.AddSuccessEvent(
            $"Successfully uploaded '{context.Message.FileName}' to Blob Storage",
            activity
        );
    }
}
