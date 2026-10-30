using Crm.Domain.Documents;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Documents;

public sealed class DocumentSettingsStateConfiguration : IEntityTypeConfiguration<DocumentSettingsState>
{
    public void Configure(EntityTypeBuilder<DocumentSettingsState> b)
    {
        b.ToTable("DocumentSettingsState", t =>
        {
            t.HasCheckConstraint("CK_DocumentSettingsState_Singleton", "\"Id\" = 1");
            t.HasCheckConstraint("CK_DocumentSettingsState_Revision", "\"Revision\" > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new DocumentSettingsStateId(x));
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.Property(x => x.ActiveIssuerVersionId).HasConversion(
            x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new IssuerSettingsVersionId(x.Value) : null);
        b.Property(x => x.ActiveTemplateVersionId).HasConversion(
            x => x.HasValue ? x.Value.Value : (long?)null,
            x => x.HasValue ? new DocumentTemplateVersionId(x.Value) : null);
        b.HasOne<IssuerSettingsVersion>().WithMany().HasForeignKey(x => x.ActiveIssuerVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DocumentTemplateVersion>().WithMany().HasForeignKey(x => x.ActiveTemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.DomainEvents);
    }
}
