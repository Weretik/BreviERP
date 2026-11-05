using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;

namespace Accounting.Infrastructure.Persistence.Configurations;

internal sealed class FinanceCounterpartyConfiguration : IEntityTypeConfiguration<FinanceCounterparty>
{
    public void Configure(EntityTypeBuilder<FinanceCounterparty> builder)
    {
        builder.ToTable("Counterparties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.CounterpartyId).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(FinanceCounterpartyDetails.NameMaxLength).IsRequired();
        builder.Property(x => x.Edrpou).HasMaxLength(FinanceCounterpartyDetails.EdrpouMaxLength).IsRequired();
        builder.Property(x => x.Iban).HasMaxLength(FinanceCounterpartyDetails.IbanMaxLength).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(FinanceCounterpartyDetails.NameMaxLength).IsRequired();
        builder.Property(x => x.Aliases).HasMaxLength(
            FinanceCounterpartyDetails.MaxAliases * (FinanceCounterpartyDetails.AliasMaxLength + 1)).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(FinanceCounterpartyDetails.NoteMaxLength).IsRequired();
        builder.Property(x => x.SearchText).HasMaxLength(FinanceCounterparty.SearchMaxLength).IsRequired();
        builder.Property(x => x.DefaultCategoryId).HasConversion(AccountingConverters.CategoryId);
        builder.Property(x => x.MergedIntoId).HasConversion(AccountingConverters.CounterpartyId);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.Details);
        builder.Ignore(x => x.Link);
        builder.Ignore(x => x.State);

        builder.HasOne<FinanceCategory>().WithMany().HasForeignKey(x => x.DefaultCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinanceCounterparty>().WithMany().HasForeignKey(x => x.MergedIntoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.Active, x.Name });
        builder.HasIndex(x => x.Edrpou);
        builder.HasIndex(x => x.Iban);
    }
}
