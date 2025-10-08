namespace Company.Project.Application.Common.Errors;

public static class DomainErrors
{
    /// <summary>
    /// Returns a new Error with the code "TaskCancelled" and the description "The request was cancelled."
    /// </summary>
    /// <param name="name">This is a one word parameter</param>
    /// <returns></returns>
    public static Error TaskCancelled(string name) =>
        Error.Failure(code: $"{name}.TaskCancelled", description: $"The request was cancelled.");

    public static Error Exception(string name, string message) =>
        Error.Failure(code: $"{name}.Exception", description: message);

    public static Error Conflict(string entity) =>
        Error.Conflict(code: $"{entity}.Conflict", description: $"{entity} already exists.");

    public static Error Conflict(string entity, string description) =>
        Error.Conflict(code: $"{entity}.Conflict", description: $"{description}");

    public static Error NotFound(string entity) =>
        Error.NotFound(code: $"{entity}.NotFound", description: $"{entity} not found");

    public static Error FileMissing(string entity, string description) =>
        Error.NotFound(code: $"{entity}.FileMissing", description: description);

    public static Error UnlockFailed(string entity, int number) =>
        Error.NotFound(
            code: $"{entity}.UnlockFailed",
            description: $"Could not unlock {entity} {number}"
        );

    public static Error FileNotFound(string message) =>
        Error.NotFound(code: "File.NotFound", description: message);

    public static Error DirectoryNotFound(string message) =>
        Error.NotFound(code: "Directory.NotFound", description: message);

    public static Error GetAllFailed(string entity) =>
        Error.Failure(code: $"{entity}.GetAllFailed", description: "Failed to fetch all records.");

    public static Error GetByIdFailed(string entity) =>
        Error.Failure(code: $"{entity}.GetByIdFailed", description: "Failed to fetch record.");

    public static Error CreationFailed(string error, string entity) =>
        Error.Failure(code: $"{entity}.CreationFailed", description: error);

    public static Error ApprovalFailed(string error, string entity) =>
        Error.Failure(code: $"{entity}.ApprovalFailed", description: error);

    public static Error UpdateFailed(string error, string entity) =>
        Error.Failure(code: $"{entity}.UpdateFailed", description: error);

    public static Error UploadFailed(string error, string fileType) =>
        Error.Failure(code: $"{fileType}.UploadFailed", description: error);

    public static Error DeleteFailed(string errorMessage, string entity) =>
        Error.Failure(code: $"{entity}.DeleteFailed", description: errorMessage);

    public static Error Paystack(string message) =>
        Error.Failure(code: $"Donation.Paystack", description: message);
}
