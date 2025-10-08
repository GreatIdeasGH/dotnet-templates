namespace Company.Project.Application.Common.Params;

public record PagingParameters
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public string? Name { get; set; }
    public string? Search { get; set; }
    public string? OrderBy { get; set; }
    public string? SortOrder { get; set; }
}
