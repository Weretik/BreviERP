namespace Ai.Application.Features.Settings.Shared;

/// <summary>Settings without the key: only whether it is set and its last four characters.</summary>
public sealed record AiSettingsResponse(
    bool HasKey,
    string? KeyHint,
    bool Enabled,
    string FastModel,
    decimal FastInputPrice,
    decimal FastOutputPrice,
    string StrongModel,
    decimal StrongInputPrice,
    decimal StrongOutputPrice,
    decimal MonthlyLimitUsd,
    int Version,
    string? UpdatedBy,
    DateTimeOffset? UpdatedAt,
    AiUsageResponse Usage,
    IReadOnlyList<AiPromptResponse> Prompts);
