namespace Ai.Domain.Settings;

/// <summary>OpenAI list price of one model in US dollars per 1 million tokens, entered by the admin.</summary>
public sealed record AiModelPrice(decimal InputPerMillion, decimal OutputPerMillion)
{
    public const decimal Max = 1_000m;

    /// <summary>Cost of one request, rounded to a millionth of a dollar.</summary>
    public decimal Cost(long inputTokens, long outputTokens) =>
        Math.Round((inputTokens * InputPerMillion + outputTokens * OutputPerMillion) / 1_000_000m, 6, MidpointRounding.AwayFromZero);
}
