using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Queries.GetQuotePdf;

/// <summary>«Сформувати КП»: the PDF of the saved КП.</summary>
public sealed record GetQuotePdfQuery(QuoteActor Actor, long Id) : IQuery<Result<QuotePdfFile>>;

public sealed record QuotePdfFile(byte[] Content, string FileName);
