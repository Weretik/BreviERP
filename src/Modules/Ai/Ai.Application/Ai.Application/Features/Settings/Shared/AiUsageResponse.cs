namespace Ai.Application.Features.Settings.Shared;

/// <summary>Use of the current month (UTC).</summary>
public sealed record AiUsageResponse(string Month, int Requests, long InputTokens, long OutputTokens, decimal CostUsd);
