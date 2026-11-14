using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Abstractions;
using Scheduling.Domain.Runs;
using Scheduling.Domain.Tasks;

namespace Scheduling.Infrastructure.Persistence;

public sealed class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options), ISchedulingDbContext
{
    public const string Schema = "scheduling";

    public DbSet<ScheduledTask> Tasks => Set<ScheduledTask>();
    public DbSet<ScheduledTaskRun> Runs => Set<ScheduledTaskRun>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(SchedulingDbContext).Assembly);
    }
}
