namespace Ai.Application.Contracts;

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

/// <summary>Use of the current month (UTC).</summary>
public sealed record AiUsageResponse(string Month, int Requests, long InputTokens, long OutputTokens, decimal CostUsd);

/// <summary>
/// One request kind: the fixed instruction the module sends (read-only, empty when unknown) and the admin's extra
/// rules (empty text when none).
/// </summary>
public sealed record AiPromptResponse(string Key, string Title, string BaseText, string Text, string? UpdatedBy, DateTimeOffset? UpdatedAt);
