using Ai.Application.Abstractions;
using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Domain.Usage;
using Microsoft.EntityFrameworkCore;

namespace Ai.Infrastructure.Persistence;

public sealed class AiDbContext(DbContextOptions<AiDbContext> options) : DbContext(options), IAiDbContext
{
    public const string Schema = "ai";

    public DbSet<AiSettings> Settings => Set<AiSettings>();
    public DbSet<AiUsageMonth> UsageMonths => Set<AiUsageMonth>();
    public DbSet<AiPrompt> Prompts => Set<AiPrompt>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(AiDbContext).Assembly);
    }
}
