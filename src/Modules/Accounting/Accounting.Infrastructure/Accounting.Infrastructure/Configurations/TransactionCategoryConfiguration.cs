using Accounting.Domain.Entities;
using Accounting.Infrastructure.Convertors;

namespace Accounting.Infrastructure.Configurations;

public sealed class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(IdConverter.TransactionCategoryIdConvert)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.ParentId)
            .HasConversion(IdConverter.TransactionCategoryIdConvert);

        builder.Property(c => c.Path)
            .HasConversion(PathConverter.Convert)
            .HasColumnType("ltree")
            .IsRequired()
            .Metadata.SetValueComparer(
                new ValueComparer<CategoryPath>(
                    (pathLeft, pathRight) => pathLeft.Value == pathRight.Value,
                    path => StringComparer.Ordinal.GetHashCode(path.Value),
                    path => CategoryPath.From(path.Value))
            );

        builder.HasIndex(c => c.Path)
            .HasMethod("gist")
            .HasDatabaseName($"IX_ProductCategories_Path_gist");
    }
}
