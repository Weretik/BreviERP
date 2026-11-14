using Crm.Domain.Embroidery.Saved;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Crm.Infrastructure.Persistence.Configurations.Embroidery;

/// <summary>«Збережені»: the form, the frozen rates and the result are jsonb documents written once.</summary>
public sealed class EmbroideryCalculationConfiguration : IEntityTypeConfiguration<EmbroiderySavedCalculation>
{
    public void Configure(EntityTypeBuilder<EmbroiderySavedCalculation> b)
    {
        b.ToTable("EmbroideryCalculations", t =>
            t.HasCheckConstraint("CK_EmbroideryCalculations_Quantity", "\"Quantity\" > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Name).HasMaxLength(EmbroiderySavedCalculation.NameMaxLength);
        b.Property(x => x.CustomerName).HasMaxLength(EmbroiderySavedCalculation.CustomerNameMaxLength);
        b.Property(x => x.CreatedByName).HasMaxLength(EmbroiderySavedCalculation.PersonNameMaxLength);
        b.Property(x => x.Mode).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Category).HasMaxLength(EmbroiderySavedCalculation.CategoryMaxLength);
        b.Ignore(x => x.Price);
        JsonColumns.Map(b.Property(x => x.Input));
        JsonColumns.Map(b.Property(x => x.Snapshot));
        JsonColumns.Map(b.Property(x => x.Result));
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.CustomerPublicId);
        b.Property(x => x.LegacyId).HasMaxLength(EmbroiderySavedCalculation.LegacyIdMaxLength);
        b.HasIndex(x => x.LegacyId).IsUnique().HasFilter("\"LegacyId\" IS NOT NULL");
    }
}
