using System.Net;
using System.Text.Json;

using GreatIdeas.Extensions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Company.Project.Infrastructure.Data;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public IPAddress? IpAddress { get; set; }
    public Dictionary<string, object> KeyValues { get; } = [];
    public Dictionary<string, object> OldValues { get; } = [];
    public Dictionary<string, object> NewValues { get; } = [];
    public AuditType AuditType { get; set; }
    public List<string> ChangedColumns { get; } = [];
    private readonly DateTimeOffset _timestamp = TimeProvider.System.GetUtcNow();

    public AuditTrail ToAudit()
    {
        var oldies = JsonSerializer.Serialize(PrepareForSerialization(OldValues));
        var newValues = JsonSerializer.Serialize(PrepareForSerialization(NewValues));
        var audit = new AuditTrail
        {
            Username = Username,
            FullName = FullName,
            Action = AuditType.ToString(),
            IpAddress = IpAddress,
            TableName = TableName,
            Timestamp = _timestamp,
            OldValues = JsonSerializer.Deserialize<JsonElement>(oldies),
            NewValues = JsonSerializer.Deserialize<JsonElement>(newValues),
            AffectedColumns = JsonSerializer.Serialize(ChangedColumns),
            Message = GenerateAuditSummary(),
        };
        return audit;
    }

    private static Dictionary<string, object> PrepareForSerialization(
        Dictionary<string, object> values
    )
    {
        var prepared = new Dictionary<string, object>();
        foreach (var kvp in values)
        {
            var value = kvp.Value;

            // Convert IPAddress objects to strings for serialization
            if (value is IPAddress ipAddress)
            {
                prepared[kvp.Key] = ipAddress.ToString();
            }
            else
            {
                prepared[kvp.Key] = value;
            }
        }
        return prepared;
    }

    private string GenerateAuditSummary()
    {
        string message;
        var tableNameNormalized = TableName.InsertSpaceBeforeUpperCase();

        switch (Entry.State)
        {
            case EntityState.Added:
                message =
                    $"{FullName} {AuditType.ToString().ToLower()}d a new {tableNameNormalized}";
                break;
            case EntityState.Modified:
                message = $"{FullName} {AuditType.ToString().ToLower()}d {tableNameNormalized}";
                break;
            case EntityState.Deleted:
                message = $"{FullName} {AuditType.ToString().ToLower()}d {tableNameNormalized}";
                break;
            default:
                message = "Unknown action 😒";
                break;
        }

        return message;
    }
}
