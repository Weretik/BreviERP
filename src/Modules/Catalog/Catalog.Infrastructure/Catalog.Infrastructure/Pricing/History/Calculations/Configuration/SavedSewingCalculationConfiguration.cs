using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Pricing.History;

internal sealed class SavedSewingCalculationConfiguration : IEntityTypeConfiguration<SavedSewingCalculationRecord>
{
    public void Configure(EntityTypeBuilder<SavedSewingCalculationRecord> builder)
    {
        builder.ToTable("SewingCalculations", "catalog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.RequestId).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.ArticleId });
        builder.Property(x => x.SnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProductId)
            .HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.Property(x => x.ArticleId)
            .HasConversion(x => x.Value, x => ArticleId.Create(x));
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SewingArticle>().WithMany().HasForeignKey(x => new { x.ProductId, x.ArticleId })
            .HasPrincipalKey(x => new { x.ProductId, ArticleId = x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
