using Crm.Infrastructure.Persistence.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentNumberSequenceConfiguration : IEntityTypeConfiguration<DocumentNumberSequence>
{
    public void Configure(EntityTypeBuilder<DocumentNumberSequence> b)
    {
        b.ToTable("DocumentNumberSequences", t =>
        {
            t.HasCheckConstraint("CK_DocumentNumberSequence_Month", "\"Month\" BETWEEN 1 AND 12");
            t.HasCheckConstraint("CK_DocumentNumberSequence_Next", "\"NextNumber\" >= 10");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        b.HasIndex(x => new { x.Year, x.Month }).IsUnique();
    }
}
