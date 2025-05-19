namespace GreatIdeas.Template.Application.Responses;

public record struct ApiItemsResponse<T>(IReadOnlyList<T> Items, int Count);    

public record struct ApiResponse(string Message);

public record struct ApiResponse<T>(string Message, T? Item);

public record struct ApiPagingResponse<T>(IReadOnlyList<T?> Items, PagedListMetaData? Metadata);

public record struct PagingMetaData
{
    public int PageCount { get; set; }
    public int TotalItemCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public bool IsFirstPage { get; set; }
    public bool IsLastPage { get; set; }
    public int FirstItemOnPage { get; set; }
    public int LastItemOnPage { get; set; }
}
