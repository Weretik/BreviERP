using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.ValueGeneration;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalog.Infrastructure.Congigurations;

public sealed class SewingMaterialLineConfiguration : IEntityTypeConfiguration<SewingMaterialLine>
{
    public void Configure(EntityTypeBuilder<SewingMaterialLine> builder)
    {
        builder.ToTable("SewingMaterialLines", "catalog", table =>
        {
            table.HasCheckConstraint("CK_SewingMaterialLines_Quantity_Positive", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_SewingMaterialLines_SortOrder_NonNegative", "\"SortOrder\" >= 0");
            table.HasCheckConstraint("CK_SewingMaterialLines_ReviewMetadata", "NOT \"IsReviewed\" OR (\"ReviewedBy\" IS NOT NULL AND length(trim(\"ReviewedBy\")) > 0 AND \"ReviewedAt\" IS NOT NULL)");
            table.HasCheckConstraint("CK_SewingMaterialLines_Reference", "(\"Kind\" = 1 AND \"FabricId\" > 0 AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NULL AND \"Unit\" = 1) OR (\"Kind\" = 2 AND \"FabricId\" IS NULL AND \"AccessoryId\" > 0 AND \"CostCode\" IS NULL AND \"Unit\" = 2) OR (\"Kind\" = 3 AND \"FabricId\" IS NULL AND \"AccessoryId\" IS NULL AND \"CostCode\" IS NOT NULL AND length(trim(\"CostCode\")) > 0 AND \"Unit\" = 3)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => CompositionLineId.Create(x)).ValueGeneratedOnAdd().HasValueGenerator<CompositionLineIdValueGenerator>();
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x)).IsRequired();
        builder.Property(x => x.ArticleId).HasConversion(new ValueConverter<ArticleId?, long?>(x => x.HasValue ? x.Value.Value : null, x => x.HasValue ? ArticleId.Create(x.Value) : null));
        builder.Property(x => x.Kind).HasConversion<int>().IsRequired();
        builder.Property(x => x.FabricId).HasConversion(x => x.HasValue ? x.Value.Value : (int?)null, x => x.HasValue ? FabricReferenceId.Create(x.Value) : null);
        builder.Property(x => x.AccessoryId).HasConversion(x => x.HasValue ? x.Value.Value : (int?)null, x => x.HasValue ? AccessoryReferenceId.Create(x.Value) : null);
        builder.Ignore(x => x.ReferenceKey);
        builder.Property(x => x.CostCode).HasMaxLength(80);
        builder.Property(x => x.Quantity).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Unit).HasConversion<int>().IsRequired();
        builder.Property(x => x.ReviewedBy).HasMaxLength(200);
        builder.HasOne<Product>().WithMany(x => x.MaterialLines).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<SewingArticle>().WithMany().HasForeignKey(x => new { x.ProductId, x.ArticleId }).HasPrincipalKey(x => new { x.ProductId, ArticleId = x.Id }).OnDelete(DeleteBehavior.Cascade);
    }
}
