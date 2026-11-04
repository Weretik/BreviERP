using Crm.Domain.Embroidery.Settings;
using Crm.Infrastructure.Persistence.Configurations.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Embroidery;

/// <summary>One row; the whole settings page is one jsonb document read and written whole.</summary>
public sealed class EmbroiderySettingsConfiguration : IEntityTypeConfiguration<EmbroiderySettings>
{
    public void Configure(EntityTypeBuilder<EmbroiderySettings> b)
    {
        b.ToTable("EmbroiderySettings", t =>
        {
            t.HasCheckConstraint("CK_EmbroiderySettings_Singleton", $"\"Id\" = {EmbroiderySettings.SingletonId}");
            t.HasCheckConstraint("CK_EmbroiderySettings_Version", "\"Version\" > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        JsonColumns.Map(b.Property(x => x.Content));
        b.Property(x => x.UpdatedByName).HasMaxLength(EmbroiderySettingsRules.NameMaxLength);
        b.Property(x => x.Version).IsConcurrencyToken();
    }
}
