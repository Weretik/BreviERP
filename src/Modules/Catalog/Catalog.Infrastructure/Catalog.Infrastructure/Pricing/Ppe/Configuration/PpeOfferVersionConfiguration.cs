using Catalog.Infrastructure.Pricing.Ppe.Records;
namespace Catalog.Infrastructure.Pricing.Ppe.Configuration;
public sealed class PpeOfferVersionConfiguration : IEntityTypeConfiguration<PpeOfferVersionRecord>
{
 public void Configure(EntityTypeBuilder<PpeOfferVersionRecord> builder)
 {
 builder.ToTable("PpePricingOfferVersions", "catalog");
 builder.HasKey(x => x.Id);
 builder.Property(x => x.Id).UseIdentityByDefaultColumn();
 builder.HasIndex(x => x.PublicId).IsUnique();
 builder.HasIndex(x => x.RequestId).IsUnique();
 builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
 builder.Property(x => x.ValuesJson).HasColumnType("jsonb").IsRequired();
 builder.Property(x => x.ApprovedBy).HasMaxLength(200);
 builder.Property(x => x.ProductId).HasConversion(x => x.Value, x => Catalog.Domain.Products.ValueObjects.ProductId.Create(x));
 builder.HasOne<Catalog.Domain.Products.Entities.Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
 builder.HasIndex(x => new { x.ProductId, x.Id });
 }
}
