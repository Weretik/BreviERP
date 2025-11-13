using Domain.Reference.Entities;
using Domain.Reference.ValueObjects;

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

        builder.Property(x => x.SeamstressCount)
            .HasColumnName("SeamstressCount")
            .IsRequired();

        builder.Property(x => x.SeamstressAverageSalary)
            .HasConversion(ReferenceConverters.MoneyConvert)
            .HasColumnName("SeamstressAverageSalary")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.MonthlyExpenses)
            .HasConversion(ReferenceConverters.MoneyConvert)
            .HasColumnName("MonthlyExpenses")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.ComplexProperty(x => x.Factors, factors =>
        {
            factors.Property(f => f.Cutting)
                .HasConversion(ReferenceConverters.PercentConvert)
                .HasColumnName("CuttingFactor")
                .HasPrecision(5, 2)
                .IsRequired();

            factors.Property(f => f.Master)
                .HasConversion(ReferenceConverters.PercentConvert)
                .HasColumnName("MasterFactor")
                .HasPrecision(5, 2)
                .IsRequired();

            factors.Property(f => f.WorkshopManager)
                .HasConversion(ReferenceConverters.PercentConvert)
                .HasColumnName("WorkshopManagerFactor")
                .HasPrecision(5, 2);

            factors.Property(f => f.SeamstressBonus)
                .HasConversion(ReferenceConverters.PercentConvert)
                .HasColumnName("SeamstressBonusFactor")
                .HasPrecision(5, 2);
        });

        builder.ComplexProperty(x => x.ProducedProfit, produced =>
        {
            ConfigureProfitFactors(produced, "Produced");
        });

        builder.ComplexProperty(x => x.PpeProfit, ppe =>
        {
            ConfigureProfitFactors(ppe, "Ppe");
        });
    }

    private static void ConfigureProfitFactors(ComplexPropertyBuilder<ProfitFactors> complex, string prefix)
    {
        complex.Property(f => f.UpTo10Units)
            .HasConversion(ReferenceConverters.PercentConvert)
            .HasColumnName($"{prefix}UpTo10UnitsProfit")
            .HasPrecision(5, 2)
            .IsRequired();

        complex.Property(f => f.TenTo40Units)
            .HasConversion(ReferenceConverters.PercentConvert)
            .HasColumnName($"{prefix}TenTo40UnitsProfit")
            .HasPrecision(5, 2)
            .IsRequired();

        complex.Property(f => f.Above40Units)
            .HasConversion(ReferenceConverters.PercentConvert)
            .HasColumnName($"{prefix}Above40UnitsProfit")
            .HasPrecision(5, 2)
            .IsRequired();
    }
}
