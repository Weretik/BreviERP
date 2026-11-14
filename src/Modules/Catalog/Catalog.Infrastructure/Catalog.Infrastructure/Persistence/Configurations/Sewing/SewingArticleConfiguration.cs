using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;


namespace Catalog.Infrastructure.Persistence.Configurations.Sewing;

public sealed class SewingArticleConfiguration : IEntityTypeConfiguration<SewingArticle>
{
    public void Configure(EntityTypeBuilder<SewingArticle> builder)
    {
        builder.ToTable("SewingArticles", "catalog", table => table.HasCheckConstraint("CK_SewingArticles_SortOrder_NonNegative", "\"SortOrder\" >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => ArticleId.Create(x)).ValueGeneratedOnAdd().HasValueGenerator<ArticleIdValueGenerator>();
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x)).IsRequired();
        builder.Property(x => x.SaleSku).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => x.SaleSku).IsUnique();
        builder.HasAlternateKey(x => new { x.ProductId, x.Id });
        builder.HasOne<Product>().WithMany(x => x.Articles).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
