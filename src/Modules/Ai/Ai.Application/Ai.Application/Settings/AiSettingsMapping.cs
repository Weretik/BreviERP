using Ai.Application.Contracts;
using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Domain.Usage;

namespace Ai.Application.Settings;

internal static class AiSettingsMapping
{
    public static AiSettingsResponse ToResponse(this AiSettings settings, AiUsageMonth? usage, string month,
        IReadOnlyList<AiPrompt> prompts) => new(
        settings.HasKey, settings.KeyHint, settings.Enabled,
        settings.FastModel, settings.FastPrice.InputPerMillion, settings.FastPrice.OutputPerMillion,
        settings.StrongModel, settings.StrongPrice.InputPerMillion, settings.StrongPrice.OutputPerMillion,
        settings.MonthlyLimitUsd, settings.Version, settings.UpdatedBy, settings.UpdatedAt,
        new AiUsageResponse(month, usage?.Requests ?? 0, usage?.InputTokens ?? 0, usage?.OutputTokens ?? 0, usage?.CostUsd ?? 0m),
        AiPromptKeys.All.Select(kind => prompts.FirstOrDefault(x => x.Id == kind.Key) is { } prompt
                ? new AiPromptResponse(kind.Key, kind.Title, prompt.Text, prompt.UpdatedBy, prompt.UpdatedAt)
                : new AiPromptResponse(kind.Key, kind.Title, string.Empty, null, null))
            .ToArray());
}
