namespace Crm.Domain.Quotes.Introductions;

public sealed class IntroductionValidationException(string code) : Exception(code);
