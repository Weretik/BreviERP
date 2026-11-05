using Ai.Domain.Settings;

namespace Ai.Application.Contracts;

/// <summary>«Перевірити»: OpenAI accepted the key and answered with this model.</summary>
public sealed record AiTestResponse(AiModelTier Tier, string Model, long InputTokens, long OutputTokens, decimal CostUsd);
