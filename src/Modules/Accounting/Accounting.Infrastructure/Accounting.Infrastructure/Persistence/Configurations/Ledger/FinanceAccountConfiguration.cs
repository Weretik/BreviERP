using Accounting.Domain.Accounts;
using Accounting.Infrastructure.Persistence;


namespace Accounting.Infrastructure.Persistence.Configurations.Ledger;

internal sealed class FinanceAccountConfiguration : IEntityTypeConfiguration<FinanceAccount>
{
    public void Configure(EntityTypeBuilder<FinanceAccount> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.AccountId).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(FinanceAccountDetails.NameMaxLength).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.Iban).HasMaxLength(FinanceAccountDetails.IbanMaxLength).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(FinanceAccountDetails.NoteMaxLength).IsRequired();
        builder.Property(x => x.CutoverConfirmedBy).HasMaxLength(FinanceAccount.ByMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.Details);
        builder.HasIndex(x => new { x.Active, x.SortOrder });
    }
}
