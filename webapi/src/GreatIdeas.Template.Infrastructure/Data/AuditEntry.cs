using System.Net;
using System.Text.Json;
using GreatIdeas.Extensions;
using GreatIdeas.Template.Domain.Entities;
using GreatIdeas.Template.Domain.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GreatIdeas.Template.Infrastructure.Data;

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
        var oldies = JsonSerializer.Serialize(OldValues);
        var newValues = JsonSerializer.Serialize(NewValues);
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
            Message = GenerateAuditSummary()
        };
        return audit;
    }

    private string GenerateAuditSummary()
    {
        string message;
        var primaryKey = GetPrimaryKeyValue(Entry);
        var columns = ChangedColumns.Count;
        var tableNameNormalized = TableName.InsertSpaceBeforeUpperCase();

        switch (Entry.State)
        {
            case EntityState.Added:
                message =
                    $"{FullName} {AuditType.ToString().ToLower()}d a new {tableNameNormalized} with {columns} columns";
                break;
            case EntityState.Modified:
                message =
                    $"{FullName} {AuditType.ToString().ToLower()}d {tableNameNormalized} with ID {primaryKey ?? "N/A"}";
                break;
            case EntityState.Deleted:
                message =
                    $"{FullName} {AuditType.ToString().ToLower()}d {tableNameNormalized} with ID {primaryKey ?? "N/A"}";
                break;
            default:
                message = "Unknown action 😒";
                break;
        }

        return message;
    }

    private static object? GetPrimaryKeyValue(EntityEntry entry)
    {
        // Get the entity type
        var entityType = entry.Metadata;

        // Get the primary key property
        var key = entityType.FindPrimaryKey();

        // Retrieve the value of the primary key
        if (key != null && key.Properties.Count > 0)
        {
            // Assuming single primary key for simplicity
            var primaryKeyValue = entry.CurrentValues[key.Properties[0]];
            return primaryKeyValue;
        }

        return null; // No primary key found
    }
}
