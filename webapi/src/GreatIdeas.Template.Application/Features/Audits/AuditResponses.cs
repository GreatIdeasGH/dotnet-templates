using GreatIdeas.Template.Application.Common.Params;
using System.Text.Json;

namespace GreatIdeas.Template.Application.Features.Audits;

public sealed record AuditDetailResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;

    public string FullName { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public JsonElement OldValues { get; set; } = default!;
    public JsonElement NewValues { get; set; } = default!;
    public string AffectedColumns { get; set; } = default!;
    public string? IpAddress { get; set; }
    public string Message { get; set; } = default!;
}

public sealed record AuditResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string Action { get; set; } = default!;
    public string Message { get; set; } = default!;
}

public sealed record AuditPagingParameters : PagingParameters
{
    public string? Action { get; set; }
    public string? TableName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
