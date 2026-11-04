namespace Crm.Application.Quotes.Pdf;

/// <summary>Draws the КП PDF; implemented in Crm.Infrastructure.</summary>
public interface IQuotePdfRenderer
{
    Task<byte[]> RenderAsync(QuotePdfModel model, CancellationToken cancellationToken);
}
