using Crm.Application.Quotes.Contracts;

namespace Crm.Application.Quotes.GetStaff;

/// <summary>Managers a КП can be given to; the Admin shows it only to Director and Admin.</summary>
public sealed record GetQuoteStaffQuery : IQuery<Result<IReadOnlyList<QuoteStaffResponse>>>;
