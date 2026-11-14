using Reference.Domain.FabricColors;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

public sealed class FabricColorConfiguration : IEntityTypeConfiguration<FabricColor>
{
    public void Configure(EntityTypeBuilder<FabricColor> builder)
    {
        builder.ToTable("FabricColors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.FabricColorIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(FabricColor.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
