using Reference.Domain.PpeMarkupRules;
using Reference.Infrastructure.Persistence;

namespace Reference.Infrastructure.Persistence.PpeMarkupRules;

public sealed class PpeMarkupRuleConfiguration : IEntityTypeConfiguration<PpeMarkupRule>
{
    public void Configure(EntityTypeBuilder<PpeMarkupRule> builder)
    {
        builder.ToTable("PpeMarkupRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(ReferenceConverters.PpeMarkupRuleIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(PpeMarkupRule.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.RetailMarkup).HasPrecision(9, 4);
        builder.Property(x => x.WholesaleMarkup).HasPrecision(9, 4);
        builder.Property(x => x.LargeWholesaleMarkup).HasPrecision(9, 4);

        builder.Property(x => x.PriceNote)
            .HasMaxLength(PpeMarkupRule.PriceNoteMaxLength);

        builder.Property(x => x.AppliesTo)
            .HasMaxLength(PpeMarkupRule.AppliesToMaxLength);
    }
}
