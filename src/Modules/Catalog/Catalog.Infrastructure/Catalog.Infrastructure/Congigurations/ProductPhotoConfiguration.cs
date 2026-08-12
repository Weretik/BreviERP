using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductPhotoConfiguration : IEntityTypeConfiguration<ProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductPhoto> builder)
    {
        builder.ToTable("ProductPhotos", "catalog");
        builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => ProductId.Create(x));
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => ProductPhotoId.Create(x)).ValueGeneratedNever();
        builder.HasKey(x => new { x.ProductId, x.Id });
        builder.Property(x => x.MediaFileId).HasConversion(x => x.Value, x => MediaFileId.Create(x)).IsRequired();
        builder.Property(x => x.Alt).HasMaxLength(300);
        builder.Property(x => x.IsVisible).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsMain).IsRequired();
        builder.HasIndex(x => new { x.ProductId, x.MediaFileId }).IsUnique();
    }
}
