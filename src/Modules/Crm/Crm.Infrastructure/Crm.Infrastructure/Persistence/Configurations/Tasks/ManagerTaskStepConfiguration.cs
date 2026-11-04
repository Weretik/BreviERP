using Crm.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Configurations.Tasks;

public sealed class ManagerTaskStepConfiguration : IEntityTypeConfiguration<ManagerTaskStep>
{
    public void Configure(EntityTypeBuilder<ManagerTaskStep> b)
    {
        b.ToTable("ManagerTaskSteps", t => t.HasCheckConstraint("CK_ManagerTaskSteps_Position", "\"Position\" >= 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Text).HasMaxLength(ManagerTaskLimits.StepTextMaxLength);
        b.HasIndex(x => new { x.TaskId, x.Position });
    }
}
