namespace Ai.Application.Features.Calls.Shared;

/// <summary>The JSON answer of the model (it matches the requested schema) and what it cost.</summary>
public sealed record AiExtraction(string Json, string Model, long InputTokens, long OutputTokens, decimal CostUsd);
