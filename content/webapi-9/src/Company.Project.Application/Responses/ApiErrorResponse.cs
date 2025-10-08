namespace Company.Project.Application.Responses;

public record struct ApiErrorResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string TraceId,
    List<string>? ErrorCodes
);

public record struct ApiValidationResponse(
    string Title,
    int Status,
    string Type,
    List<string>? Errors
);
