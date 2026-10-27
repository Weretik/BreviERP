namespace Crm.Domain.Quotes;

public sealed class QuoteValidationException(string message) : Exception(message);
