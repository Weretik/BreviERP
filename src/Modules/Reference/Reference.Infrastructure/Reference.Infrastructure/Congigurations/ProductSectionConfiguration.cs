using Reference.Domain.ProductSections;
using Reference.Infrastructure.Converters;

namespace Reference.Infrastructure.Congigurations;

public sealed class ProductSectionConfiguration : IEntityTypeConfiguration<ProductSection>
{
    public void Configure(EntityTypeBuilder<ProductSection> builder)
    {
        builder.ToTable("ProductSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.ProductSectionIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(ProductSection.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Comment)
            .HasMaxLength(ProductSection.CommentMaxLength);
    }
}
