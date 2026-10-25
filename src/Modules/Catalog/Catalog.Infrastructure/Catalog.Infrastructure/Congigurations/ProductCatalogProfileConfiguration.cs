using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductCatalogProfileConfiguration : IEntityTypeConfiguration<ProductCatalogProfile>
{
    public void Configure(EntityTypeBuilder<ProductCatalogProfile> builder)
    {
        builder.ToTable("ProductCatalogProfiles", "catalog", table =>
            table.HasCheckConstraint("CK_ProductCatalogProfiles_SupplierPrice", "\"SupplierPrice\" IS NULL OR \"SupplierPrice\" > 0"));
        builder.HasKey(x => x.ProductId);
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.HasOne<Product>().WithOne(x => x.CatalogProfile).HasForeignKey<ProductCatalogProfile>(x => x.ProductId);
        builder.Property(x => x.SaleSku).HasMaxLength(80);
        builder.HasIndex(x => x.SaleSku).IsUnique();
        builder.Property(x => x.Season).HasMaxLength(100);
        builder.Property(x => x.Unit).HasMaxLength(40);
        builder.Property(x => x.WebsiteUrl).HasMaxLength(2048);
        builder.Property(x => x.SourceReviewDate).HasMaxLength(100);
        builder.Property(x => x.SupplierSku).HasMaxLength(200);
        builder.Property(x => x.SupplierProductUrl).HasMaxLength(2048);
        builder.Property(x => x.SupplierPrice).HasPrecision(18, 6);
        builder.Property(x => x.PricingRuleCode).HasMaxLength(200);
        builder.Property(x => x.ShortDescription).HasMaxLength(4000);
        builder.Property(x => x.SupplierId).HasConversion(
            new ValueConverter<SupplierReferenceId?, int?>(
                x => x.HasValue ? x.Value.Value : null,
                x => x.HasValue ? SupplierReferenceId.Create(x.Value) : null));
    }
}
