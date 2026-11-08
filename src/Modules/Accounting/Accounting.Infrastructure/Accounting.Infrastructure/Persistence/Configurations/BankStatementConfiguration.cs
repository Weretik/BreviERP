using Accounting.Domain.Accounts;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;

namespace Accounting.Infrastructure.Persistence.Configurations;

internal sealed class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("BankStatements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.StatementId).ValueGeneratedNever();
        builder.Property(x => x.AccountId).HasConversion(AccountingConverters.AccountId);
        builder.Property(x => x.FileName).HasMaxLength(BankStatement.FileNameMaxLength).IsRequired();
        builder.Property(x => x.FileHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CreatedBy).HasMaxLength(MovementRules.ByMaxLength).IsRequired();
        builder.Property(x => x.BookedBy).HasMaxLength(MovementRules.ByMaxLength);
        builder.Property(x => x.Version).IsConcurrencyToken();

        // A link inside the module: an account with statements is only archived.
        builder.HasOne<FinanceAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        // The same file of the same account opens the statement uploaded before.
        builder.HasIndex(x => new { x.AccountId, x.FileHash }).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
    }
}
