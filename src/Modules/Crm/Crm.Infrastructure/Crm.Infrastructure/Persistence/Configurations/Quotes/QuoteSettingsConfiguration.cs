using System.Text.Json;
using Crm.Domain.Quotes.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

/// <summary>One row; each settings section is a jsonb document read and written whole.</summary>
public sealed class QuoteSettingsConfiguration : IEntityTypeConfiguration<QuoteSettings>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<QuoteSettings> b)
    {
        b.ToTable("QuoteSettings", t =>
        {
            t.HasCheckConstraint("CK_QuoteSettings_Singleton", $"\"Id\" = {QuoteSettings.SingletonId}");
            t.HasCheckConstraint("CK_QuoteSettings_Version", "\"Version\" > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        JsonColumn(b.Property(x => x.Company));
        JsonColumn(b.Property(x => x.Email));
        JsonColumn(b.Property(x => x.Pdf));
        JsonColumn(b.Property(x => x.Introduction));
        JsonColumn(b.Property(x => x.LetterVariants));
        JsonColumn(b.Property(x => x.StaffContacts));
        b.Property(x => x.UpdatedByName).HasMaxLength(QuoteSettingsLimits.ShortTextMaxLength);
        b.Property(x => x.Version).IsConcurrencyToken();
        b.Ignore(x => x.Content);
    }

    private static void JsonColumn<T>(PropertyBuilder<T> property) where T : class =>
        property.HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value, Json),
                text => JsonSerializer.Deserialize<T>(text, Json)!,
                new ValueComparer<T>(
                    (left, right) => JsonSerializer.Serialize(left, Json) == JsonSerializer.Serialize(right, Json),
                    value => JsonSerializer.Serialize(value, Json).GetHashCode(StringComparison.Ordinal),
                    value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Json), Json)!));
}
