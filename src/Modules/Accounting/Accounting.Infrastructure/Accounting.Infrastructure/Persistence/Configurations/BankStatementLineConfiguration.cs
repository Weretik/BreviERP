using Accounting.Domain.Counterparties;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;

namespace Accounting.Infrastructure.Persistence.Configurations;

internal sealed class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("BankStatementLines", t =>
            t.HasCheckConstraint("CK_BankStatementLines_Counterparty",
                "(\"CounterpartyKind\" IS NULL) = (\"CounterpartyId\" IS NULL)"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.StatementLineId).ValueGeneratedNever();
        builder.Property(x => x.StatementId).HasConversion(AccountingConverters.StatementId);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(x => x.BankName).HasMaxLength(BankLineData.NameMaxLength).IsRequired();
        builder.Property(x => x.Edrpou).HasMaxLength(BankLineData.CodeMaxLength).IsRequired();
        builder.Property(x => x.Iban).HasMaxLength(BankLineData.CodeMaxLength).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(BankLineData.PurposeMaxLength).IsRequired();
        builder.Property(x => x.Document).HasMaxLength(BankLineData.DocumentMaxLength).IsRequired();
        builder.Property(x => x.BankId).HasMaxLength(BankLineData.BankIdMaxLength);
        builder.Property(x => x.Fingerprint).HasMaxLength(64);
        builder.Property(x => x.Error).HasMaxLength(BankLineData.ErrorMaxLength).IsRequired();
        builder.Property(x => x.RawJson).HasMaxLength(BankLineData.RawJsonMaxLength).IsRequired();
        builder.Property(x => x.Duplicate).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.DuplicateNote).HasMaxLength(BankLineData.ErrorMaxLength).IsRequired();
        builder.Property(x => x.TransferAccountId).HasConversion(AccountingConverters.AccountId);
        builder.Property(x => x.CounterpartyKind).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CounterpartyId).HasMaxLength(CounterpartyLink.IdMaxLength);
        builder.Property(x => x.CounterpartyName).HasMaxLength(FinanceCounterpartyDetails.NameMaxLength).IsRequired();
        builder.Property(x => x.CategoryId).HasConversion(AccountingConverters.CategoryId);
        builder.Property(x => x.OneoffName).HasMaxLength(FinanceTransactionDetails.OneoffNameMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(FinanceTransactionDetails.DescriptionMaxLength).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(FinanceTransactionDetails.CommentMaxLength).IsRequired();
        builder.Property(x => x.State).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.TransactionId).HasConversion(AccountingConverters.TransactionId);
        builder.Property(x => x.TransferId).HasConversion(AccountingConverters.TransferId);
        builder.Ignore(x => x.Counterparty);
        builder.Ignore(x => x.HasParty);
        builder.Ignore(x => x.WillBook);

        // The line belongs to its statement. The suggested category and the other account of a transfer are not linked:
        // they are only a draft until booking, which checks them again.
        builder.HasOne<BankStatement>().WithMany().HasForeignKey(x => x.StatementId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.StatementId, x.RowNumber });
        builder.HasIndex(x => new { x.StatementId, x.State });
        builder.HasIndex(x => x.TransactionId);
        builder.HasIndex(x => x.TransferId);
    }
}
