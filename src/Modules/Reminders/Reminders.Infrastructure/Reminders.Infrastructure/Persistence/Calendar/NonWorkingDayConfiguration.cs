using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reminders.Domain.Calendar;

namespace Reminders.Infrastructure.Persistence.Calendar;

public sealed class NonWorkingDayConfiguration : IEntityTypeConfiguration<NonWorkingDay>
{
    public void Configure(EntityTypeBuilder<NonWorkingDay> b)
    {
        b.ToTable("NonWorkingDays");
        b.HasKey(x => x.Date);
        b.Property(x => x.Note).HasMaxLength(200);
    }
}
