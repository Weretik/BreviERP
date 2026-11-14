using Ai.Domain.Settings;
using Ai.Application.Features.Calls.Shared;

namespace Ai.Application.Features.Settings.Commands.TestAiConnection;

/// <summary>«Перевірити»: a tiny request to the model of the tier proves the key and the model name.</summary>
public sealed record TestAiConnectionCommand(AiModelTier Tier) : ICommand<Result<AiTestResponse>>;
