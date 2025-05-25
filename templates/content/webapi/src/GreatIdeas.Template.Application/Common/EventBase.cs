namespace GreatIdeas.Template.Application.Common;

public abstract record EventBase
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; init; } = TimeProvider.System.GetUtcNow();
}