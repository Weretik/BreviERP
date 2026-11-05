using Ai.Application.Abstractions;
using Ai.Application.Contracts;
using Ai.Domain.Settings;
using Ai.Domain.Usage;

namespace Ai.Application.Settings;

/// <summary>Loads the single settings row (adding the defaults on first write) and builds the response with this month's use.</summary>
public sealed class AiSettingsStore(IAiDbContext db, TimeProvider clock, IEnumerable<IAiBasePrompt> basePrompts)
{
    public async Task<AiSettings> LoadForUpdateAsync(CancellationToken cancellationToken)
    {
        var settings = db.Settings.Local.FirstOrDefault(x => x.Id == AiSettings.OpenAi)
            ?? await db.Settings.FirstOrDefaultAsync(x => x.Id == AiSettings.OpenAi, cancellationToken);
        if (settings is not null) return settings;
        settings = AiSettings.CreateDefault();
        db.Settings.Add(settings);
        return settings;
    }

    public async Task<AiSettingsResponse> ResponseAsync(AiSettings settings, CancellationToken cancellationToken)
    {
        var month = AiUsageMonth.IdOf(clock.GetUtcNow());
        var usage = await db.UsageMonths.AsNoTracking().FirstOrDefaultAsync(x => x.Id == month, cancellationToken);
        var prompts = await db.Prompts.AsNoTracking().ToListAsync(cancellationToken);
        return settings.ToResponse(usage, month, prompts, basePrompts);
    }
}
