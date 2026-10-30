using Crm.Domain.Documents.EmailTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.EmailTemplates;
public sealed class EmailTemplateStateConfiguration : IEntityTypeConfiguration<EmailTemplateState>
{
    public void Configure(EntityTypeBuilder<EmailTemplateState> b)
    {
        b.ToTable("EmailTemplateState", t => { t.HasCheckConstraint("CK_EmailTemplateState_Singleton", "\"Id\" = 1"); t.HasCheckConstraint("CK_EmailTemplateState_Revision", "\"Revision\" > 0"); });
        b.HasKey(x => x.Id); b.Property(x => x.Id).HasConversion(x => x.Value, x => new EmailTemplateStateId(x)).ValueGeneratedNever();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.ActiveVersionId).HasConversion(x => x.HasValue ? x.Value.Value : (long?)null, x => x.HasValue ? new EmailTemplateVersionId(x.Value) : null);
        b.HasOne<EmailTemplateVersion>().WithMany().HasForeignKey(x => x.ActiveVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}
