using Accounting.Domain.Categories;
using Accounting.Infrastructure.Persistence;


namespace Accounting.Infrastructure.Persistence.Configurations.Ledger;

internal sealed class FinanceCategoryConfiguration : IEntityTypeConfiguration<FinanceCategory>
{
    public void Configure(EntityTypeBuilder<FinanceCategory> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(AccountingConverters.CategoryId).ValueGeneratedNever();
        builder.Property(x => x.ParentId).HasConversion(AccountingConverters.CategoryId);
        builder.Property(x => x.Name).HasMaxLength(FinanceCategory.NameMaxLength).IsRequired();
        builder.Property(x => x.Direction).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Note).HasMaxLength(FinanceCategory.NoteMaxLength).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.IsRoot);
        // Self link inside the module: a root cannot be removed while a subcategory points to it.
        builder.HasOne<FinanceCategory>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.Scope, x.Direction, x.ParentId });
    }
}
