namespace Crm.Domain.Quotes;

public sealed class QuoteRevisionException : Exception
{
    public QuoteRevisionException() : base("revision_conflict") { }
}
