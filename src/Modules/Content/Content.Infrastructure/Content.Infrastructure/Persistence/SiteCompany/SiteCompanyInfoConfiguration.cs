using System.Text.Json;
using Content.Domain.ProductPages;
using Content.Domain.SiteCompany;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Content.Infrastructure.Persistence.SiteCompany;

/// <summary>
/// One row; the two-language texts, the schedule, the managers, the bank details and the links are small documents
/// kept as JSON text, read and written only as a whole.
/// </summary>
public sealed class SiteCompanyInfoConfiguration : IEntityTypeConfiguration<SiteCompanyInfo>
{
    public void Configure(EntityTypeBuilder<SiteCompanyInfo> b)
    {
        b.ToTable("SiteCompany", t => t.HasCheckConstraint("CK_SiteCompany_Single", "\"Id\" = 1"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Phone).HasMaxLength(20);
        b.Property(x => x.Email).HasMaxLength(SiteCompanyInfo.EmailMaxLength);
        b.Property(x => x.MapQuery).HasMaxLength(SiteCompanyInfo.TextMaxLength);
        Json(b.Property(x => x.City));
        Json(b.Property(x => x.Street));
        Json(b.Property(x => x.Schedule));
        Json(b.Property(x => x.Managers));
        Json(b.Property(x => x.BankDetails));
        Json(b.Property(x => x.Socials));
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.UpdatedBy).HasMaxLength(ProductPageRules.UpdatedByMaxLength);
    }

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static void Json<T>(PropertyBuilder<T> property) where T : class =>
        property.HasConversion(
                value => JsonSerializer.Serialize(value, Options),
                text => JsonSerializer.Deserialize<T>(text, Options)!,
                new ValueComparer<T>(
                    (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
                    value => JsonSerializer.Serialize(value, Options).GetHashCode(StringComparison.Ordinal),
                    value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Options), Options)!))
            .IsRequired();
}
