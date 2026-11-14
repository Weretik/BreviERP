using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reminders.Domain.Occurrences;


namespace Reminders.Infrastructure.Persistence.Configurations;

public sealed class ReminderActionConfiguration : IEntityTypeConfiguration<ReminderAction>
{
    public void Configure(EntityTypeBuilder<ReminderAction> b)
    {
        b.ToTable("OccurrenceActions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Kind).HasConversion<int>();
        b.OwnsOne(x => x.Actor, a =>
        {
            ReminderOccurrenceConfiguration.ConfigureActor(a, "Actor");
            a.Property(x => x.Name).IsRequired();
        });
        b.Navigation(x => x.Actor).IsRequired();
    }
}
