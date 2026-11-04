using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

/// <summary>A value stored as one jsonb document, read and written whole and compared by its JSON.</summary>
internal static class JsonColumns
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static void Map<T>(PropertyBuilder<T> property) where T : class =>
        property.HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value, Json),
                text => JsonSerializer.Deserialize<T>(text, Json)!,
                new ValueComparer<T>(
                    (left, right) => JsonSerializer.Serialize(left, Json) == JsonSerializer.Serialize(right, Json),
                    value => JsonSerializer.Serialize(value, Json).GetHashCode(StringComparison.Ordinal),
                    value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Json), Json)!));
}
