using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.ValueGeneration;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalog.Infrastructure.Congigurations;

public sealed class SewingOperationLineConfiguration : IEntityTypeConfiguration<SewingOperationLine>
{
    public void Configure(EntityTypeBuilder<SewingOperationLine> builder)
    {
        builder.ToTable("SewingOperationLines", "catalog", table =>
        {
            table.HasCheckConstraint("CK_SewingOperationLines_Quantity_Positive", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_SewingOperationLines_ManualMinutes_Positive", "\"ManualMinutes\" IS NULL OR \"ManualMinutes\" > 0");
            table.HasCheckConstraint("CK_SewingOperationLines_SharedOnly", "\"ArticleId\" IS NULL");
            table.HasCheckConstraint("CK_SewingOperationLines_SortOrder_NonNegative", "\"SortOrder\" >= 0");
            table.HasCheckConstraint("CK_SewingOperationLines_OperationId_Positive", "\"OperationId\" > 0");
            table.HasCheckConstraint("CK_SewingOperationLines_ReviewMetadata", "NOT \"IsReviewed\" OR (\"ReviewedBy\" IS NOT NULL AND length(trim(\"ReviewedBy\")) > 0 AND \"ReviewedAt\" IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => CompositionLineId.Create(x)).ValueGeneratedOnAdd().HasValueGenerator<CompositionLineIdValueGenerator>();
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x)).IsRequired();
        builder.Property(x => x.ArticleId).HasConversion(new ValueConverter<ArticleId?, long?>(x => x.HasValue ? x.Value.Value : null, x => x.HasValue ? ArticleId.Create(x.Value) : null));
        builder.Property(x => x.OperationId).HasConversion(x => x.Value, x => OperationReferenceId.Create(x)).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.ManualMinutes).HasPrecision(18, 6);
        builder.Property(x => x.ReviewedBy).HasMaxLength(200);
        builder.HasIndex(x => new { x.ProductId, x.OperationId });
        builder.HasOne<Product>().WithMany(x => x.OperationLines).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
