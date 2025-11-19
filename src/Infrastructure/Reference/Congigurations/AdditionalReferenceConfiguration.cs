using Domain.Reference.Entities;

namespace Infrastructure.Reference.Congigurations;

public sealed class AdditionalReferenceConfiguration : IEntityTypeConfiguration<AdditionalReference>
{
    public void Configure(EntityTypeBuilder<AdditionalReference> builder)
    {
        builder.ToTable("AdditionalReferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.AdditionalReferenceIdConvert)
            .ValueGeneratedNever();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);
    }
}
