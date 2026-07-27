using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Infrastructure.Congigurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => ProductId.Create(x))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RuName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => ProductSlug.Create(x))
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.OwnsMany(x => x.Photos, photoBuilder =>
        {
            photoBuilder.ToTable("ProductPhotos", "catalog");
            photoBuilder.WithOwner().HasForeignKey("ProductId");
            photoBuilder.HasKey("ProductId", "Id");

            photoBuilder.Property(x => x.Id)
                .HasConversion(x => x.Value, x => ProductPhotoId.Create(x))
                .ValueGeneratedNever();

            photoBuilder.Property(x => x.MediaFileId)
                .HasConversion(x => x.Value, x => MediaFileId.Create(x))
                .IsRequired();

            photoBuilder.Property(x => x.Alt)
                .HasMaxLength(300);

            photoBuilder.Property(x => x.IsVisible).IsRequired();
            photoBuilder.Property(x => x.SortOrder).IsRequired();
            photoBuilder.Property(x => x.IsMain).IsRequired();

            photoBuilder.HasIndex("ProductId", nameof(ProductPhoto.MediaFileId)).IsUnique();
        });

        builder.OwnsMany(x => x.Categories, categoryBuilder =>
        {
            categoryBuilder.ToTable("ProductCategoryLinks", "catalog");
            categoryBuilder.WithOwner().HasForeignKey("ProductId");
            categoryBuilder.HasKey("ProductId", "Id");

            categoryBuilder.Property(x => x.Id)
                .HasColumnName("CategoryId")
                .HasConversion(x => x.Value, x => ProductCategoryId.Create(x))
                .ValueGeneratedNever();

            categoryBuilder.Ignore(x => x.CategoryId);

            categoryBuilder.HasOne<ProductCategory>()
                .WithMany()
                .HasForeignKey("Id")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
