using System.Text.Json.Serialization;

namespace Accounting.Domain.Audit;

/// <summary>Which finance record a «Журнал змін» entry is about.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AuditEntity>))]
public enum AuditEntity
{
    Account,
    Category,
}
