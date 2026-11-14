using Crm.Application.Features.Quotes.Shared;

namespace Crm.Application.Features.Quotes.Queries.GetQuoteStaff;

/// <summary>Managers a КП can be given to; the Admin shows it only to Director and Admin.</summary>
public sealed record GetQuoteStaffQuery : IQuery<Result<IReadOnlyList<QuoteStaffResponse>>>;
