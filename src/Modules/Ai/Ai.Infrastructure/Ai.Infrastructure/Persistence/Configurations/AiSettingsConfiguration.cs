using Ai.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Ai.Infrastructure.Persistence.Configurations;

public sealed class AiSettingsConfiguration : IEntityTypeConfiguration<AiSettings>
{
    public void Configure(EntityTypeBuilder<AiSettings> b)
    {
        b.ToTable("Settings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(40);
        b.Property(x => x.ProtectedApiKey).HasMaxLength(4_000);
        b.Property(x => x.KeyHint).HasMaxLength(4);
        b.Property(x => x.FastModel).HasMaxLength(AiSettings.ModelMaxLength);
        b.Property(x => x.StrongModel).HasMaxLength(AiSettings.ModelMaxLength);
        b.ComplexProperty(x => x.FastPrice, p =>
        {
            p.Property(x => x.InputPerMillion).HasColumnName("FastInputPrice").HasPrecision(10, 4);
            p.Property(x => x.OutputPerMillion).HasColumnName("FastOutputPrice").HasPrecision(10, 4);
        });
        b.ComplexProperty(x => x.StrongPrice, p =>
        {
            p.Property(x => x.InputPerMillion).HasColumnName("StrongInputPrice").HasPrecision(10, 4);
            p.Property(x => x.OutputPerMillion).HasColumnName("StrongOutputPrice").HasPrecision(10, 4);
        });
        b.Property(x => x.MonthlyLimitUsd).HasPrecision(10, 2);
        b.Property(x => x.UpdatedBy).HasMaxLength(AiSettings.UpdatedByMaxLength);
        b.Ignore(x => x.HasKey);
    }
}
