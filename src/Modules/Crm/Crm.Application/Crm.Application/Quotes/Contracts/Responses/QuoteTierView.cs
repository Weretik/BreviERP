namespace Crm.Application.Quotes.Contracts.Responses;

public sealed record QuoteTierView(string Code, decimal? Amount, bool Visible, string Currency);
