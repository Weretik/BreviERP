using Crm.Domain.Leads;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Leads;

/// <summary>Seeds the call results of the old sheet (LEAD-001 R-06); the director changes them on «Довідники».</summary>
public sealed class LeadCallResultConfiguration : IEntityTypeConfiguration<LeadCallResult>
{
    public void Configure(EntityTypeBuilder<LeadCallResult> b)
    {
        b.ToTable("LeadCallResults", t =>
            t.HasCheckConstraint("CK_LeadCallResults_RetryDays",
                "(\"Outcome\" = 'Retry' AND \"RetryDays\" BETWEEN 1 AND 60) OR (\"Outcome\" <> 'Retry' AND \"RetryDays\" IS NULL)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Name).HasMaxLength(LeadLimits.ResultNameMaxLength);
        b.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(20);

        b.HasData(
            Seed(1, "Не дозвонились", LeadOutcome.Retry, 1, 10),
            Seed(2, "Не відповідають", LeadOutcome.Retry, 2, 20),
            Seed(3, "Передзвонити", LeadOutcome.Callback, null, 30),
            Seed(4, "Вийшли на відповідального", LeadOutcome.Talking, null, 40),
            Seed(5, "Надіслали пропозицію", LeadOutcome.Offer, null, 50),
            Seed(6, "Перенесено в CRM", LeadOutcome.Client, null, 60),
            Seed(7, "Відмова", LeadOutcome.Closed, null, 70),
            Seed(8, "Нецільове підприємство", LeadOutcome.Closed, null, 80));
    }

    private static object Seed(long id, string name, LeadOutcome outcome, int? retryDays, int position) =>
        new { Id = id, Name = name, Outcome = outcome, RetryDays = retryDays, IsActive = true, Position = position };
}
