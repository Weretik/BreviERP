using Production.Domain.FabricNorms;

namespace Production.Infrastructure.Persistence.Configurations;

internal sealed class ModelFabricNormConfiguration : IEntityTypeConfiguration<ModelFabricNorm>
{
    private const int Precision = 7;

    public void Configure(EntityTypeBuilder<ModelFabricNorm> builder)
    {
        builder.ToTable("FabricNorms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.ModelCode).HasMaxLength(ModelFabricNorm.ModelCodeMaxLength).IsRequired();
        builder.HasIndex(x => x.ModelCode).IsUnique();
        builder.Ignore(x => x.Values);
        foreach (var name in new[]
                 {
                     nameof(ModelFabricNorm.OneColor), nameof(ModelFabricNorm.TwoColorsMain), nameof(ModelFabricNorm.TwoColorsAdditional),
                     nameof(ModelFabricNorm.ThreeColorsMain), nameof(ModelFabricNorm.ThreeColorsAdditional1), nameof(ModelFabricNorm.ThreeColorsAdditional2),
                 })
            builder.Property(name).HasPrecision(Precision, FabricNormValues.Decimals);
    }
}
