using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Domain.Usage;

namespace Ai.Application.Abstractions;

public interface IAiDbContext
{
    DbSet<AiSettings> Settings { get; }
    DbSet<AiUsageMonth> UsageMonths { get; }
    DbSet<AiPrompt> Prompts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
