using Domain.Accounting.Entities;
using Domain.Accounting.ValueObjects;
using Infrastructure.Accounting.Convertors;

namespace Infrastructure.Accounting.Configurations;

public sealed class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        CategoryMapping.Configure<TransactionCategoryId, TransactionCategory>(
            builder, tableName: "TransactionCategories");

        builder.Property(c => c.Id)
            .HasConversion(IdConverter.TransactionCategoryIdConvert)
            .ValueGeneratedNever();
    }
}
