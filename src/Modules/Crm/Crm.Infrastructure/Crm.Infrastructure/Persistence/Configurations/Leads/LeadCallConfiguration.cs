using Crm.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Crm.Infrastructure.Persistence.Configurations.Leads;

public sealed class LeadCallConfiguration : IEntityTypeConfiguration<LeadCall>
{
    public void Configure(EntityTypeBuilder<LeadCall> b)
    {
        b.ToTable("LeadCalls");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.ByName).HasMaxLength(LeadLimits.NameMaxLength);
        b.Property(x => x.ResultName).HasMaxLength(LeadLimits.ResultNameMaxLength);
        b.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Comment).HasMaxLength(LeadLimits.CommentMaxLength);
        b.HasIndex(x => x.At);
        b.HasIndex(x => new { x.ByUserId, x.At });
        // The result by id without a foreign key: the call keeps the result's name at the time of the call.
    }
}
