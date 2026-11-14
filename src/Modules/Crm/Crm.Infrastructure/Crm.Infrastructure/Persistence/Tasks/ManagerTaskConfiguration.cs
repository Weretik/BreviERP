using Crm.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Infrastructure.Persistence.Tasks;

public sealed class ManagerTaskConfiguration : IEntityTypeConfiguration<ManagerTask>
{
    public void Configure(EntityTypeBuilder<ManagerTask> b)
    {
        b.ToTable("ManagerTasks", t =>
        {
            t.HasCheckConstraint("CK_ManagerTasks_Version", "\"Version\" > 0");
            t.HasCheckConstraint("CK_ManagerTasks_Completed",
                "(\"Status\" = 'Done' AND \"CompletedAt\" IS NOT NULL) OR (\"Status\" <> 'Done' AND \"CompletedAt\" IS NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.Title).HasMaxLength(ManagerTaskLimits.TitleMaxLength);
        b.Property(x => x.Description).HasMaxLength(ManagerTaskLimits.DescriptionMaxLength);
        b.Property(x => x.AssigneeName).HasMaxLength(ManagerTaskLimits.NameMaxLength);
        b.Property(x => x.AuthorName).HasMaxLength(ManagerTaskLimits.NameMaxLength);
        b.Property(x => x.CustomerName).HasMaxLength(ManagerTaskLimits.NameMaxLength);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Version).IsConcurrencyToken();

        b.HasIndex(x => new { x.AssigneeUserId, x.Status, x.DueAt });
        b.HasIndex(x => new { x.Status, x.DueAt });
        b.HasIndex(x => x.CompletedAt);
        // The lead of a «Подзвонити» task (LEAD-001 R-08), without a foreign key: tasks never block leads.
        b.HasIndex(x => new { x.LeadId, x.Status });

        // A Crm customer by its public id, without a foreign key: the KeepinCRM sync may replace customers.
        b.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Steps).HasField("_steps").UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasMany(x => x.Events).WithOne().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Events).HasField("_events").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
