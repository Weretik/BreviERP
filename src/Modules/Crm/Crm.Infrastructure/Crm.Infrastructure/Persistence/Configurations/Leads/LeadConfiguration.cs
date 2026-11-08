using Crm.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Leads;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> b)
    {
        b.ToTable("Leads", t =>
        {
            t.HasCheckConstraint("CK_Leads_Version", "\"Version\" > 0");
            t.HasCheckConstraint("CK_Leads_SourceId", "\"SourceId\" > 0");
            t.HasCheckConstraint("CK_Leads_Owner", "(\"OwnerUserId\" IS NULL) = (\"OwnerName\" IS NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.NameKey).HasMaxLength(LeadLimits.ShortText);
        b.HasIndex(x => x.NameKey).IsUnique();
        b.HasIndex(x => x.SourceId);

        b.Property(x => x.Category).HasMaxLength(LeadLimits.CategoryMaxLength);
        b.Property(x => x.Company).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.Region).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.Industry).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.Scale).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.Signal).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.FactOrPlan).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.SignalUrl).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.VolumeReason).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.Offer).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.Website).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.Contacts).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.WhomToCall).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.TenderCheck).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.CallReason).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.Reliability).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.Email).HasMaxLength(LeadLimits.ShortText);
        b.Property(x => x.SheetNotes).HasMaxLength(LeadLimits.LongText);
        b.Property(x => x.OwnerName).HasMaxLength(LeadLimits.NameMaxLength);
        b.Property(x => x.LastResultName).HasMaxLength(LeadLimits.ResultNameMaxLength);
        b.Property(x => x.Stage).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Version).IsConcurrencyToken();

        b.HasIndex(x => new { x.OwnerUserId, x.Stage, x.NextContactAt });
        b.HasIndex(x => new { x.Stage, x.OwnerUserId });
        b.HasIndex(x => x.Category);

        b.HasMany(x => x.Calls).WithOne().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Calls).HasField("_calls").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
