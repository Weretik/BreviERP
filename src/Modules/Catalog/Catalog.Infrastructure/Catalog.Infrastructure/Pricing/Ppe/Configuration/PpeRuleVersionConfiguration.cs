using Catalog.Infrastructure.Pricing.Ppe.Records;
namespace Catalog.Infrastructure.Pricing.Ppe.Configuration;
public sealed class PpeRuleVersionConfiguration : IEntityTypeConfiguration<PpeRuleVersionRecord>
{
 public void Configure(EntityTypeBuilder<PpeRuleVersionRecord> builder)
 {
 builder.ToTable("PpePricingRuleVersions", "catalog");
 builder.HasKey(x => x.Id);
 builder.Property(x => x.Id).UseIdentityByDefaultColumn();
 builder.HasIndex(x => x.PublicId).IsUnique();
 builder.HasIndex(x => x.RequestId).IsUnique();
 builder.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
 builder.Property(x => x.ValuesJson).HasColumnType("jsonb").IsRequired();
 builder.Property(x => x.ApprovedBy).HasMaxLength(200);
 builder.Property(x => x.Code).HasMaxLength(200).IsRequired();
 builder.HasIndex(x => new { x.Code, x.Id });
 }
}
