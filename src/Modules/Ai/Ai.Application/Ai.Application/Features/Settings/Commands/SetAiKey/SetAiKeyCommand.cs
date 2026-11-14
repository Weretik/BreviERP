using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.SetAiKey;

/// <summary>Stores a new OpenAI key encrypted; the key itself is never returned or logged.</summary>
public sealed record SetAiKeyCommand(string ApiKey, string By) : ICommand<Result<AiSettingsResponse>>
{
    public override string ToString() => $"{nameof(SetAiKeyCommand)} {{ By = {By} }}";
}
