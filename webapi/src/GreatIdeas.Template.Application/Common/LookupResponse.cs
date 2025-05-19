namespace GreatIdeas.Template.Application.Common;

public sealed record LookupResponse
{
    public object Id { get; set; } = default!;
    public string Name { get; set; } = default!;
}
