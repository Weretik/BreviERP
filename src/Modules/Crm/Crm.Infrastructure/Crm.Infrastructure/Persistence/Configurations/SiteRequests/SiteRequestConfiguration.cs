using Crm.Domain.SiteRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.SiteRequests;

/// <summary>Requests of the public site (LEAD-002).</summary>
public sealed class SiteRequestConfiguration : IEntityTypeConfiguration<SiteRequest>
{
    public void Configure(EntityTypeBuilder<SiteRequest> b)
    {
        b.ToTable("SiteRequests");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Phone).HasMaxLength(SiteRequest.PhoneMaxLength).IsRequired();
        b.Property(x => x.Workwear).HasMaxLength(SiteRequest.WorkwearMaxLength);
        b.Property(x => x.Quantity).HasMaxLength(SiteRequest.QuantityMaxLength);
        b.Property(x => x.Comment).HasMaxLength(SiteRequest.CommentMaxLength);
        b.Property(x => x.Locale).HasMaxLength(2).IsRequired();
        b.Property(x => x.FileName).HasMaxLength(SiteRequest.FileNameMaxLength);
        b.Property(x => x.FileUrl).HasMaxLength(SiteRequest.UrlMaxLength);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.OwnerName).HasMaxLength(SiteRequest.NameMaxLength);
        b.Property(x => x.Note).HasMaxLength(SiteRequest.NoteMaxLength);
        b.Property(x => x.NotifyError).HasMaxLength(SiteRequest.ErrorMaxLength);
        b.Property(x => x.Version).IsConcurrencyToken();
        b.Ignore(x => x.IsOpen);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.OwnerUserId);
    }
}
