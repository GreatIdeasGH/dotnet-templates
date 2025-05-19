using System.Net;
using System.Text.Json;

namespace GreatIdeas.Template.Domain.Entities;

public sealed class AuditTrail
{
    public Guid AuditTraiId { get; set; }
    public string Username { get; set; } = default!;

    public string FullName { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
    public JsonElement OldValues { get; set; } = default!;
    public JsonElement NewValues { get; set; } = default!;
    public string AffectedColumns { get; set; } = default!;
    public IPAddress? IpAddress { get; set; }
    public string Message { get; set; } = default!;
}
