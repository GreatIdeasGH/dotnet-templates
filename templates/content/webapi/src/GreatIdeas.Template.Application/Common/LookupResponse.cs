namespace GreatIdeas.Template.Application.Common;

public sealed record LookupResponse
{
    public object Id { get; set; } = null!;
    public string Name { get; set; } = null!;
}