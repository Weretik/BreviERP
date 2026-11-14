namespace Ai.Application.Features.Settings.Shared;

/// <summary>
/// One request kind: the fixed instruction the module sends (read-only, empty when unknown) and the admin's extra
/// rules (empty text when none).
/// </summary>
public sealed record AiPromptResponse(string Key, string Title, string BaseText, string Text, string? UpdatedBy, DateTimeOffset? UpdatedAt);
