namespace Infrastructure.Accounting.Configurations;

public sealed class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
        => CategoryMapping.Configure<TransactionCategory, TransactionCategoryId>(
            builder, tableName: "TransactionCategories");
}
