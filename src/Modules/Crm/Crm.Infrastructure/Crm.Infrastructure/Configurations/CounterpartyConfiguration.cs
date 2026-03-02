using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Configurations;

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
