using Crm.Application.Quotes.Pdf.Introduction;

namespace Crm.Application.Quotes.Pdf;

/// <summary>Draws the КП PDF; implemented in Crm.Infrastructure.</summary>
public interface IQuotePdfRenderer
{
    Task<byte[]> RenderAsync(QuotePdfModel model, CancellationToken cancellationToken);

    /// <summary>The «КП для знайомства» catalogue (R-16).</summary>
    Task<byte[]> RenderIntroductionAsync(QuoteIntroPdfModel model, CancellationToken cancellationToken);
}
