namespace GreatIdeas.Template.Application.Responses;

public record struct ApiItemsResponse<T>(IReadOnlyList<T> Items, int Count);

public record struct ApiResponse(string Message);

public record struct ApiResponse<T>(string Message, T? Item);

public record struct ApiPagingResponse<T>(IReadOnlyList<T?> Items, PagedListMetaData? Metadata);