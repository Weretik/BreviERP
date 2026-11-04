namespace Crm.Domain.Quotes;

/// <summary>«Під запит» or «Для знайомства» (stage 4).</summary>
public enum QuoteKind
{
    Request = 1,
    Introduction = 2
}

/// <summary>A КП is a draft until it is sent (stage 3).</summary>
public enum QuoteStatus
{
    Draft = 1,
    Sent = 2
}

/// <summary>What an item of a КП is built from.</summary>
public enum QuoteItemKind
{
    /// <summary>A Catalog sewing model with one or more fabric variants.</summary>
    Sewing = 1,

    /// <summary>A Catalog PPE product with one price row.</summary>
    Ppe = 2,

    /// <summary>A PPE item that is not in the Catalog; the manager types its prices.</summary>
    OneOffPpe = 3
}
