using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Infrastructure.Persistence.Configurations;

internal sealed class FinanceTransactionConfiguration : IEntityTypeConfiguration<FinanceTransaction>
{
    public void Configure(EntityTypeBuilder<FinanceTransaction> builder)
    {
        builder.ToTable("Transactions", t => t.HasCheckConstraint("CK_Transactions_Amount", "\"AmountMinor\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.TransactionId).ValueGeneratedNever();
        builder.Property(x => x.AccountId).HasConversion(AccountingConverters.AccountId);
        builder.Property(x => x.CategoryId).HasConversion(AccountingConverters.CategoryId);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(x => x.OneoffName).HasMaxLength(FinanceTransactionDetails.OneoffNameMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(FinanceTransactionDetails.DescriptionMaxLength).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(FinanceTransactionDetails.CommentMaxLength).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Verification).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CreatedBy).HasMaxLength(MovementRules.ByMaxLength).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.Details);

        // Links inside the module: an account or a category with operations cannot be removed.
        builder.HasOne<FinanceAccount>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FinanceCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Scope, x.Status, x.Date, x.Id });
        builder.HasIndex(x => new { x.AccountId, x.Status, x.Scope, x.Date });
        builder.HasIndex(x => x.CategoryId);
    }
}
