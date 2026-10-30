using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Quotes;

public sealed class QuoteVersionConfiguration : IEntityTypeConfiguration<QuoteVersion>
{
    public void Configure(EntityTypeBuilder<QuoteVersion> builder)
    {
        builder.ToTable("QuoteVersions", table =>
            table.HasCheckConstraint("CK_QuoteVersions_Number", "\"VersionNumber\" > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.QuoteId).HasConversion(x => x.Value, x => new QuoteId(x));
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.RecipientName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.TaxBasis).HasMaxLength(40).IsRequired();
        builder.Property(x => x.IntroductionPublicSnapshot).HasConversion(
            x => x == null ? null : IntroductionPublicSnapshotJson.Serialize(x),
            x => x == null ? null : IntroductionPublicSnapshotJson.Deserialize(x))
            .HasColumnType("jsonb").IsRequired(false);
        builder.HasOne<Quote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<QuoteVersion>().WithMany()
            .HasForeignKey(x => new { x.QuoteId, x.ParentVersionId })
            .HasPrincipalKey(x => new { x.QuoteId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.VersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.QuoteId, x.VersionNumber }).IsUnique();
    }
}
