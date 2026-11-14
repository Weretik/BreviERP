using Accounting.Domain.Accounts;
using Accounting.Domain.Movements;
using Accounting.Domain.Transfers;
using Accounting.Infrastructure.Persistence;

namespace Accounting.Infrastructure.Persistence.Transfers;

internal sealed class FinanceTransferConfiguration : IEntityTypeConfiguration<FinanceTransfer>
{
    public void Configure(EntityTypeBuilder<FinanceTransfer> builder)
    {
        builder.ToTable("Transfers", t =>
        {
            t.HasCheckConstraint("CK_Transfers_Amount", "\"AmountMinor\" > 0");
            t.HasCheckConstraint("CK_Transfers_DifferentAccounts", "\"FromAccountId\" <> \"ToAccountId\"");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.TransferId).ValueGeneratedNever();
        builder.Property(x => x.FromAccountId).HasConversion(AccountingConverters.AccountId);
        builder.Property(x => x.ToAccountId).HasConversion(AccountingConverters.AccountId);
        builder.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.Note).HasMaxLength(FinanceTransferDetails.NoteMaxLength).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CreatedBy).HasMaxLength(MovementRules.ByMaxLength).IsRequired();
        builder.Property(x => x.ExternalId).HasMaxLength(LegacyMovement.ExternalIdMaxLength);
        builder.Property(x => x.Fingerprint).HasMaxLength(LegacyMovement.FingerprintMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.Details);

        builder.HasOne<FinanceAccount>().WithMany().HasForeignKey(x => x.FromAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinanceAccount>().WithMany().HasForeignKey(x => x.ToAccountId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Scope, x.Status, x.Date, x.Id });
        builder.HasIndex(x => x.FromAccountId);
        builder.HasIndex(x => x.ToAccountId);
        builder.HasIndex(x => x.Fingerprint);
        builder.HasIndex(x => x.ExternalId);
    }
}
