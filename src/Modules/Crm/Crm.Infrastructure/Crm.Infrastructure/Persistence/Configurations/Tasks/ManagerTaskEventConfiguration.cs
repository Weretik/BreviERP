using Crm.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Tasks;

public sealed class ManagerTaskEventConfiguration : IEntityTypeConfiguration<ManagerTaskEvent>
{
    public void Configure(EntityTypeBuilder<ManagerTaskEvent> b)
    {
        b.ToTable("ManagerTaskEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ByName).HasMaxLength(ManagerTaskLimits.NameMaxLength);
        b.Property(x => x.Details).HasMaxLength(ManagerTaskLimits.DetailsMaxLength);
        b.HasIndex(x => new { x.TaskId, x.At });
    }
}
