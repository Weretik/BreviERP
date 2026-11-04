namespace Crm.Application.Quotes.Sending.Gmail;

/// <summary>Where the Google callback sends the browser back: the Admin address, for example https://office.brevi.com.ua.</summary>
public sealed record QuoteMailOptions(string AdminUrl);
