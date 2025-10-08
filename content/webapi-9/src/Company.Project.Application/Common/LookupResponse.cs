namespace Company.Project.Application.Common;

public sealed record LookupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
