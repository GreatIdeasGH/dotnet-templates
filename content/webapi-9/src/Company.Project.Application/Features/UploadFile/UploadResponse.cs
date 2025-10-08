namespace Company.Project.Application.Features.UploadFile;

public record UploadFileResponse(bool Success, string FilePath, string Thumbnail);

public record struct UploadParameters(string FileName);
