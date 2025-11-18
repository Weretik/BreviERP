using Domain.CRM.Entities;

namespace Infrastructure.CRM.Configurations;

public class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
{
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.ToTable("Counterparties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(CrmConverter.CounterpartyIdConvert)
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasConversion(CrmConverter.CounterpartyTypeConvert)
            .HasMaxLength(50)
            .HasColumnName("Type")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}
