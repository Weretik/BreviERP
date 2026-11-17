using BuildingBlocks.Domain.Entity;
namespace Ai.Domain.Usage;

/// <summary>OpenAI use of one calendar month (UTC), counted from the token usage of each answer and the prices in settings.</summary>
public sealed class AiUsageMonth : IAggregateRoot
{
    private AiUsageMonth() { }

    private AiUsageMonth(string id) => Id = new AiUsageMonthId(id);

    public static AiUsageMonth Create(string id) =>
        new(id);

    /// <summary><c>yyyy-MM</c> in UTC.</summary>
    public AiUsageMonthId Id { get; private set; }

    public int Requests { get; private set; }
    public long InputTokens { get; private set; }
    public long OutputTokens { get; private set; }
    public decimal CostUsd { get; private set; }

    public static string IdOf(DateTimeOffset at) => at.UtcDateTime.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
}
