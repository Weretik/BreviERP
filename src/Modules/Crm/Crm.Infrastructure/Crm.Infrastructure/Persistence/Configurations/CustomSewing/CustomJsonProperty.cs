using System.Text.Json;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Drafts;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.CustomSewing;

internal static class CustomJsonProperty
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
    private static T Decode<T>(string json)
    {
        var value = JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException("Invalid custom snapshot.");
        return value switch { CustomConfiguration configuration => (T)(object)configuration.Freeze(),
            CustomPriceBasis basis => (T)(object)basis.Freeze(),
            CustomTemplateOrigin origin => (T)(object)origin.Validate(), _ => value };
    }
    internal static void Configure<T>(PropertyBuilder<T> property) where T : class
    {
        property.HasConversion(x => Encode(x), x => Decode<T>(x)).HasColumnType("jsonb").IsRequired();
        property.Metadata.SetValueComparer(new ValueComparer<T>((left, right) => Encode(left) == Encode(right),
            value => Encode(value).GetHashCode(), value => Decode<T>(Encode(value))));
    }

    internal static void ConfigureOptional<T>(PropertyBuilder<T?> property) where T : class
    {
        property.HasConversion(x => x == null ? null : Encode(x), x => x == null ? null : Decode<T>(x))
            .HasColumnType("jsonb").IsRequired(false);
        property.Metadata.SetValueComparer(new ValueComparer<T?>(
            (left, right) => Encode(left) == Encode(right), value => Encode(value).GetHashCode(),
            value => value == null ? null : Decode<T>(Encode(value))));
    }
}
