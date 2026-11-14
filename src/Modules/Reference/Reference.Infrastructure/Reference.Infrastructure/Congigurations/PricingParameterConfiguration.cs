using Reference.Domain.PricingParameters;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

public sealed class PricingParameterConfiguration : IEntityTypeConfiguration<PricingParameter>
{
    public void Configure(EntityTypeBuilder<PricingParameter> builder)
    {
        builder.ToTable("PricingParameters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.PricingParameterIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasMaxLength(PricingParameter.CodeMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Group)
            .HasMaxLength(PricingParameter.GroupMaxLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(PricingParameter.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasPrecision(18, 6);

        builder.Property(x => x.Unit)
            .HasMaxLength(PricingParameter.UnitMaxLength)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(PricingParameter.CommentMaxLength);
    }
}
