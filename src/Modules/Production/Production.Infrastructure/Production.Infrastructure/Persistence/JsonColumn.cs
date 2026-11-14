using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Production.Infrastructure.Persistence;

/// <summary>A list kept as one JSON column (jsonb in PostgreSQL), compared by its JSON text.</summary>
internal static class JsonColumn
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static PropertyBuilder<List<T>> AsJson<T>(this PropertyBuilder<List<T>> property) =>
        property.HasColumnType("jsonb")
            .HasConversion(
                new ValueConverter<List<T>, string>(
                    value => JsonSerializer.Serialize(value, Options),
                    json => JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>()),
                new ValueComparer<List<T>>(
                    (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
                    value => JsonSerializer.Serialize(value, Options).GetHashCode(StringComparison.Ordinal),
                    value => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(value, Options), Options)!))
            .IsRequired();
}
