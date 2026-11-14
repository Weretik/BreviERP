using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.RemoveAiKey;

public sealed record RemoveAiKeyCommand(string By) : ICommand<Result<AiSettingsResponse>>;
