using Ai.Contracts.Prompts;
using Ai.Application.Abstractions;
using Ai.Domain.Prompts;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiPrompt;

/// <summary>Saves the extra rules of one request kind; empty text removes them.</summary>
public sealed record UpdateAiPromptCommand(string Key, string? Text, string By) : ICommand<Result<AiSettingsResponse>>;
