using Crm.Application.Quotes.Access;

namespace Crm.Application.Quotes.Pdf.GetPdf;

/// <summary>«Сформувати КП»: the PDF of the saved КП.</summary>
public sealed record GetQuotePdfQuery(QuoteActor Actor, long Id) : IQuery<Result<QuotePdfFile>>;

public sealed record QuotePdfFile(byte[] Content, string FileName);
