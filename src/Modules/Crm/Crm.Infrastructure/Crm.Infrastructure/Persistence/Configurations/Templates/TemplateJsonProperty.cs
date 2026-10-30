using System.Text.Json;
using Crm.Domain.CustomSewing.Templates;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Crm.Infrastructure.Persistence.Configurations.Templates;

internal static class TemplateJsonProperty
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static string Encode(TemplateSelections value) => JsonSerializer.Serialize(value, Json);
    private static TemplateSelections Decode(string value) => (JsonSerializer.Deserialize<TemplateSelections>(value, Json)
        ?? throw new InvalidOperationException("Invalid template selections.")).Freeze();
    internal static void Configure(PropertyBuilder<TemplateSelections> property)
    {
        property.HasConversion(x => Encode(x), x => Decode(x)).HasColumnType("jsonb").IsRequired();
        property.Metadata.SetValueComparer(new ValueComparer<TemplateSelections>((a, b) => Encode(a!) == Encode(b!),
            x => Encode(x).GetHashCode(), x => Decode(Encode(x))));
    }
}
