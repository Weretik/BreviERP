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

/// <summary>Extra rules of one request kind; empty text when none.</summary>
public sealed record AiPromptResponse(string Key, string Title, string Text, string? UpdatedBy, DateTimeOffset? UpdatedAt);
