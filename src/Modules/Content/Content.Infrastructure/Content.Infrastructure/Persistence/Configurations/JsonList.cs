using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Infrastructure.Persistence.Configurations;

/// <summary>Stores an ordered read-only list as one jsonb column; two lists are equal when their JSON is equal.</summary>
internal static class JsonList
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static PropertyBuilder<IReadOnlyList<T>> IsJsonList<T>(this PropertyBuilder<IReadOnlyList<T>> property)
    {
        property.HasColumnType("jsonb").HasConversion(
            list => JsonSerializer.Serialize(list, Options),
            json => JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>(),
            new ValueComparer<IReadOnlyList<T>>(
                (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
                list => JsonSerializer.Serialize(list, Options).GetHashCode(),
                list => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(list, Options), Options)!));
        return property;
    }
}
