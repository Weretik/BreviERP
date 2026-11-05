using System.Text.Json.Serialization;

namespace Accounting.Domain.Audit;

/// <summary>What happened to the record.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AuditAction>))]
public enum AuditAction
{
    Create,
    Update,
    Archive,
    Restore,
    Delete,
    CutoverConfirm,
    CutoverReset,
}
