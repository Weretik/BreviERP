namespace Crm.Domain.Quotes.Pricing;

public sealed class QuoteTier
{
    public long Id { get; private set; }
    public long VariantId { get; private set; }
    public string Code { get; private set; } = null!;
    public decimal? Amount { get; private set; }
    public bool Visible { get; private set; }
    public string Currency { get; private set; } = "UAH";

    private QuoteTier() { }

    public static QuoteTier Create(string code, decimal? amount, bool visible)
    {
        if (code is not ("retail" or "wholesale" or "largeWholesale") ||
            amount is < 0 || (amount is { } value && decimal.Round(value, 2) != value))
            throw new QuoteValidationException("invalid_tier");
        return new QuoteTier { Code = code, Amount = amount, Visible = visible };
    }
}
