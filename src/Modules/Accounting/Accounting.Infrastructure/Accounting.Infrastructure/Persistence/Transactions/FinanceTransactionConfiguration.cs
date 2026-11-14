using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Infrastructure.Persistence;

namespace Accounting.Infrastructure.Persistence.Transactions;

internal sealed class FinanceTransactionConfiguration : IEntityTypeConfiguration<FinanceTransaction>
{
    public void Configure(EntityTypeBuilder<FinanceTransaction> builder)
    {
        builder.ToTable("Transactions", t =>
        {
            t.HasCheckConstraint("CK_Transactions_Amount", "\"AmountMinor\" > 0");
            // A counterparty is a kind and an id together; clients and suppliers live in other modules (no link).
            t.HasCheckConstraint("CK_Transactions_Counterparty",
                "(\"CounterpartyKind\" IS NULL) = (\"CounterpartyId\" IS NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.TransactionId).ValueGeneratedNever();
        builder.Property(x => x.AccountId).HasConversion(AccountingConverters.AccountId);
        builder.Property(x => x.CategoryId).HasConversion(AccountingConverters.CategoryId);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.CounterpartyKind).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CounterpartyId).HasMaxLength(CounterpartyLink.IdMaxLength);
        builder.Property(x => x.OneoffName).HasMaxLength(FinanceTransactionDetails.OneoffNameMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(FinanceTransactionDetails.DescriptionMaxLength).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(FinanceTransactionDetails.CommentMaxLength).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Verification).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CreatedBy).HasMaxLength(MovementRules.ByMaxLength).IsRequired();
        builder.Property(x => x.ExternalId).HasMaxLength(LegacyMovement.ExternalIdMaxLength);
        builder.Property(x => x.Fingerprint).HasMaxLength(LegacyMovement.FingerprintMaxLength);
        builder.Property(x => x.BankTransactionId).HasMaxLength(LegacyMovement.ExternalIdMaxLength);
        builder.Property(x => x.BankPurpose).HasMaxLength(FinanceTransactionDetails.CommentMaxLength).IsRequired()
            .HasDefaultValue(string.Empty);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.Details);
        builder.Ignore(x => x.Counterparty);

        // Links inside the module: an account or a category with operations cannot be removed.
        builder.HasOne<FinanceAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinanceCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Scope, x.Status, x.Date, x.Id });
        builder.HasIndex(x => new { x.AccountId, x.Status, x.Scope, x.Date });
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => new { x.CounterpartyKind, x.CounterpartyId });
        builder.HasIndex(x => new { x.Verification, x.Date });
        // A later bank statement finds lines it already has by these.
        builder.HasIndex(x => x.Fingerprint);
        builder.HasIndex(x => x.ExternalId);
    }
}
